using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace .VendorIntegrationService.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }

            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return enumerationValue.ToString();
        }

        public static bool TryParseDescription<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            var enumType = typeof(TEnum);
            var enumValues = enumType.GetEnumValues();
            foreach (TEnum enumVal in enumValues)
            {
                var memberInfo = enumType.GetMember(enumVal.ToString()).First();
                var descriptionAttribute =
                    memberInfo.GetCustomAttribute<DescriptionAttribute>();

                if (
                    descriptionAttribute != null && descriptionAttribute.Description == value ||
                    enumVal.ToString() == value
                )
                {
                    result = enumVal;
                    return true;
                }
            }

            result = default(TEnum);
            return false;
        }
    }
}