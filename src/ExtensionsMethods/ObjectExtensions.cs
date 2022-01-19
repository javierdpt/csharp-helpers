using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace .VendorIntegrationService.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static int ToIntOrDefault(this string s, int defaultValue = 0)
        {
            return int.TryParse(s, out var r) ? r : defaultValue;
        }

        public static DateTime GetValueOrFirstOfTheMonth(this DateTime? d)
        {
            return d ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public static string GetFormUrlEncoded<T>(this T self, params string[] ignore) where T : class
        {
            var type = self.GetType();
            return string.Join(
                "&",
                (
                    from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where !ignore.Contains(pi.Name)
                    let name = pi.Name
                    let value = type.GetProperty(pi.Name)?.GetValue(self, null)
                    select $"{name}={value}"
                ).ToArray()
            );
        }

        public static bool EqualPublicInstanceProperties<T>(this T self, T to, params string[] ignore) where T : class
        {
            if (self == null || to == null) return self == to;

            var type = typeof(T);
            return !(
                from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !ignore.Contains(pi.Name)
                let selfValue = type.GetProperty(pi.Name)?.GetValue(self, null)
                let toValue = type.GetProperty(pi.Name)?.GetValue(to, null)
                where selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue))
                select selfValue
            ).Any();
        }

        public static bool UpdateIfChangedPublicInstanceProperties<T>(this T self, T to, params string[] ignore) where T : class
        {
            var changed = false;
            if (self == null || to == null) return false;

            var type = typeof(T);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!ignore.Contains(prop.Name))
                {
                    var selfValue = type.GetProperty(prop.Name)?.GetValue(self, null);
                    var toValue = type.GetProperty(prop.Name)?.GetValue(to, null);
                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                    {
                        type.GetProperty(prop.Name)?.SetValue(self, toValue, null);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        public static List<object> GetPropValues(this object obj, string name)
        {
            var resp = new List<object>();

            var pathArr = name.Split('.');
            for (var i = 0; i < pathArr.Length; i++)
            {
                var part = pathArr[i];
                if (obj == null) { return new List<object>(); }

                if (obj is IEnumerable enumerableObj)
                {
                    foreach (var innerObj in enumerableObj)
                    {
                        var nextName = string.Join('.', pathArr.Skip(i));
                        resp.AddRange(GetPropValues(innerObj, nextName));
                    }
                }

                var type = obj.GetType();
                var info = type.GetProperty(part);
                if (info == null) { return resp.Where(v => v != null).ToList(); }

                obj = info.GetValue(obj, null);
            }

            resp.Add(obj);

            return resp.Where(v => v != null).ToList();
        }

        public static List<T> GetPropValues<T>(this object obj, string name)
        {
            var res = GetPropValues(obj, name);

            if (typeof(T) == typeof(string))
                res = res.Select(v => v.ToString() as object).ToList();

            return res.Cast<T>().ToList();
        }

        public static object GetPropValue(this object obj, string name)
        {
            foreach (var part in name.Split('.'))
            {
                if (obj == null) { return null; }

                var type = obj.GetType();
                var info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this object obj, string name)
        {
            var res = GetPropValue(obj, name);
            if (res == null) { return default; }

            if (typeof(T) == typeof(string))
                res = res.ToString();

            return (T)res;
        }
    }
}