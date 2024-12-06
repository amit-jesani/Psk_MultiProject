using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace ProductWeight
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if the entity returned by the query is an actual record.
        /// </summary>
        /// <param name="entity">The entity reference.</param>
        public static T Clone<T>(this T entity, string[] doNotCopyEntityAttribute) where T : Entity
        {
            Entity clone = new Entity(entity.LogicalName);

            foreach (KeyValuePair<string, object> attr in entity.Attributes)
            {
                if (!doNotCopyEntityAttribute.Contains(attr.Key.ToLower()))
                {
                    clone[attr.Key] = attr.Value;
                }
            }

            return clone.ToEntity<T>();
        }

        /// <summary>
        /// Checks if the entity returned by the query is an actual record.
        /// </summary>
        /// <param name="entity">The entity reference.</param>
        public static bool IsValid(this Entity entity)
        {
            if (entity == null || String.IsNullOrEmpty(entity.LogicalName) || entity.Id == Guid.Empty)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Extension method to set the value of an attribute.
        /// </summary>
        /// <param name="entity">The entity reference.</param>
        /// <param name="attribute">The name attribute.</param>
        /// <param name="value">The new value of the attribute.</param>
        public static void SetValue(this Entity entity, string attribute, object value)
        {
            if (entity != null)
            {
                if (entity.Attributes.Contains(attribute))
                {
                    entity.Attributes[attribute] = value;
                }
                else
                {
                    entity.Attributes.Add(attribute, value);
                }
            }
        }


        public static T GetSetDefaultValue<T>(Entity entity, Entity preEntity, string attribute, T defaultValue)
        {
            T result = defaultValue;

            if (entity.Attributes.Contains(attribute))
            {
                if (entity[attribute] is AliasedValue && typeof(T) != typeof(AliasedValue))
                {
                    result = (T)entity.GetAttributeValue<AliasedValue>(attribute).Value;
                }
                else
                {
                    result = (T)entity[attribute];
                }
            }
            else if (preEntity != null && preEntity.Attributes.Contains(attribute))
            {
                if (preEntity[attribute] is AliasedValue && typeof(T) != typeof(AliasedValue))
                {
                    result = (T)preEntity.GetAttributeValue<AliasedValue>(attribute).Value;
                }
                else
                {
                    result = (T)preEntity[attribute];
                }
            }
            else
            {
                result = defaultValue;
            }
            return result;
        }
        /// <summary>
        /// Gets the value of an attribute.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="entity">The entity reference.</param>
        /// <param name="attribute">The attribute to get the value from.</param>
        /// <param name="defaultValue">The default value to return if the attribute is null.</param>
        /// <returns></returns>
        public static T GetValue<T>(this Entity entity, string attribute, T defaultValue)
        {
            T result = defaultValue;

            if (entity != null)
            {
                if (entity.Attributes.Contains(attribute))
                {
                    result = entity.GetAttributeValue<T>(attribute);

                    if (EqualityComparer<T>.Default.Equals(result, default(T)))
                    {
                        result = defaultValue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Tries the get value of an attribute.
        /// </summary>
        /// <typeparam name="T">Type contained by the attribute</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="attrValue">The attribute value.</param>
        /// <returns>If the attribute exists or not</returns>
        public static bool TryGetValue<T>(this Entity entity, string attribute, out T attrValue)
        {
            bool result = false;

            attrValue = default(T);

            if (entity != null)
            {
                if (entity.Attributes.Contains(attribute))
                {
                    attrValue = entity.GetAttributeValue<T>(attribute);

                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Truncates a string to a maximum length specified.
        /// </summary>
        /// <param name="str">The string to be truncated.</param>
        /// <param name="maxLength">The maximum length of the string.</param>
        /// <returns>The string truncated to the specified length</returns>
        public static string Truncate(this string str, int maxLength)
        {
            if (str.Length <= maxLength)
            {
                return str;
            }
            else { return str.Substring(0, maxLength); }
        }

        /// <summary>
        /// Get the value of an attribute from the target entity or the pre-image or return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The target entity.</param>
        /// <param name="preEntity">The pre-image entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>the attribute value</returns>
        
    }
}
