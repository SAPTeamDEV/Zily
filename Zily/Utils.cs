using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents extension methods for enums, specially the <see cref="HeaderFlag"/>.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks whether the flag is a response flag.
        /// </summary>
        /// <param name="flag">
        /// The response flag.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the flag is a response flag, otherwise it returns <see langword="false"/>.
        /// </returns>
        public static bool IsResponse(this HeaderFlag flag)
        {
            return flag.GetAttributeOfType<ResponseFlagAttribute>() != null;
        }

        /// <summary>
        /// Checks whether the flag is a request flag.
        /// </summary>
        /// <param name="flag">
        /// The response flag.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the flag is a request flag, otherwise it returns <see langword="false"/>.
        /// </returns>
        public static bool IsRequest(this HeaderFlag flag)
        {
            return flag.GetAttributeOfType<RequestFlagAttribute>() != null;
        }

        /// <summary>
        /// Checks whether the flag is parameterless.
        /// </summary>
        /// <param name="flag">
        /// The response flag.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the flag is parameterless, otherwise it returns <see langword="false"/>.
        /// </returns>
        public static bool IsParameterless(this HeaderFlag flag)
        {
            return flag.GetAttributeOfType<FlagAttribute>().IsParameterless;
        }

        /// <summary>
        /// Gets an attribute of an enum field value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the attribute you want to retrieve.
        /// </typeparam> 
        /// <param name="enumVal">
        /// The enum value.
        /// </param>
        /// <returns>
        /// The attribute of type <typeparamref name="T"/> that exists on the enum value.
        /// </returns>
        /// <example>
        /// <![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]>
        /// </example>
        public static T GetAttributeOfType<T>(this Enum enumVal)
            where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
