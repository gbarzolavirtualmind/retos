using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDumper
{
    public class ObjectDumper<T>
    {
        private Dictionary<string, MulticastDelegate> _converters = new Dictionary<string, MulticastDelegate>();
        public void AddTemplateFor<TProperty>(Expression<Func<T, TProperty>> propertyName, Func<TProperty, string> converter)
        {
            var member = propertyName.Body as MemberExpression;
            var name = member.Member.Name;
            
            if (_converters.ContainsKey(name))
            {
                _converters[name] = converter;
                return;
            }
            _converters.Add(name, converter);
        
        }


        public IEnumerable<KeyValuePair<string,string>> Dump(T instance)
        {
            return instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(x => x.CanRead)
                .OrderBy(x => x.Name)
                .Select(x => new KeyValuePair<string, string>(x.Name, GetValue(instance,x)));
        }
        private string GetValue(T instance, PropertyInfo property) 
        {
            var hasConverter = _converters.ContainsKey(property.Name);
            if (!hasConverter)
            {
                return property.GetValue(instance).ToString();
            }

            var propertyValue = property.GetValue(instance);

            var converter = _converters[property.Name];
            return converter.DynamicInvoke(propertyValue).ToString();
            //return converter(instance);
            
        }
    }
}
