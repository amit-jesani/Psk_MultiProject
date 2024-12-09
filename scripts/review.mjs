import { Octokit } from "@octokit/rest";
import { Configuration, OpenAIApi } from "openai";

const openaiApiKey = process.env.OPENAI_API_KEY;
const githubToken = process.env.GITHUB_TOKEN;
const repoName = process.env.REPO_NAME;
const prNumber = process.env.PR_NUMBER;

(async () => {
  try {
    // Initialize Octokit and OpenAI
    const octokit = new Octokit({ auth: githubToken });
    const openai = new OpenAIApi(new Configuration({ apiKey: openaiApiKey }));

    const [owner, repo] = repoName.split("/");

    if (!owner || !repo || !prNumber) {
      throw new Error(`Missing parameters: owner=${owner}, repo=${repo}, prNumber=${prNumber}`);
    }

    // Fetch modified files in the PR
    const filesResponse = await octokit.pulls.listFiles({
      owner,
      repo,
      pull_number: parseInt(prNumber, 10),
    });

    if (!filesResponse.data || filesResponse.data.length === 0) {
      console.log("No files modified in this PR.");
      return;
    }

    for (const file of filesResponse.data) {
      // Skip irrelevant or non-text files
      if (
        !file.filename.match(/\.(cs|js|ts|py|java|cpp|c|go|rb)$/) || // Only specific extensions
        file.status !== "added" && file.status !== "modified" // Only added or modified files
      ) {
        console.log(`Skipping file: ${file.filename}`);
        continue;
      }

      console.log(`Processing file: ${file.filename}`);

      // Review the patch (changes)
      if (file.patch) {
        try {
          const patchReviewPrompt = `Review the following changes in the file "${file.filename}". Focus on bugs, improvements, syntax and optimizations. Be concise:\n\n${file.patch}`;
          const patchReviewResponse = await openai.createChatCompletion({
            model: "gpt-4-turbo",
            messages: [{ role: "user", content: patchReviewPrompt }],
            temperature: 0.2,
            max_tokens: 500,
          });

          const patchReviewComments = patchReviewResponse.data.choices[0].message.content;
          await octokit.issues.createComment({
            owner,
            repo,
            issue_number: parseInt(prNumber, 10),
            body: `### Review for Changes in \`${file.filename}\`:\n\n\`\`\`diff\n${file.patch}\n\`\`\`\n\n${patchReviewComments}`,
          });
        } catch (err) {
          console.error(`Error reviewing patch for file: ${file.filename}`, err.message);
          continue;
        }
      }

      // Review the entire file
      try {
        const contentResponse = await octokit.repos.getContent({
          owner,
          repo,
          path: file.filename,
        });

        const fileContent = Buffer.from(contentResponse.data.content, "base64").toString("utf-8");

        const fullFileReviewPrompt = `Review the entire file "${file.filename}" with a focus on the latest modified code. Analyze the modifications, suggest improvements, and optimize the updated code. Ignore any removed code and ensure the review addresses potential issues, performance optimizations, and coding standards in the latest version:\n\n${fileContent}`;

        const fullFileReviewResponse = await openai.createChatCompletion({
          model: "gpt-4-turbo",
          messages: [{ role: "user", content: fullFileReviewPrompt }],
          temperature: 0.3,
          max_tokens: 1000,
        });

        const fullFileReviewComments = fullFileReviewResponse.data.choices[0].message.content;
        await octokit.issues.createComment({
          owner,
          repo,
          issue_number: parseInt(prNumber, 10),
          body: `### Additional Context for \`${file.filename}\`:\n\n${fullFileReviewComments}`,
        });
      } catch (err) {
        console.error(`Error reviewing file: ${file.filename}`, err.message);
        continue;
      }
    }
  } catch (error) {
    console.error("Error during code review:", error.message);
    process.exit(1);
  }
})();
