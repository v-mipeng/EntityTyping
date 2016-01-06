using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pml.type
{
    public class Property    :Dictionary<Object,Object>
    {
        /* Construct a property object
         */
        public Property() : base()
        {
        }
           
        /*Construct a property object and add an attribute to this object
         */
        public Property(string key, string value)     :base()
        {
            if (IsValidInput(key, value))
            {
                base[key] = value;
            }
        }

        /*Set property with string type of key-value pair
         */
        public void SetProperty(String key, string value)
        {
            base[key] = value;
        }

        /*Get property value with string key
         */ 
        public String GetProperty(String key)
        {
            Object obj;
            if (base.TryGetValue(key, out obj))
            {
                if (obj is string)
                {
                    return (string)obj;
                }
            }
            return null;
        }


        /* Add an attribute to this property
         */
        public void Set(Object key, Object value)
        {
            if (IsValidInput(key, value))
            {
                base[key] = value;
            }
        }

        /*Get attribute of given key
         */ 
        public Object Get(Object key)
        {
            Object obj;
            return base.TryGetValue(key, out obj) ? obj : null;
        }

        /*Get attribute of given key or return defaultValue if not exists.
         */ 
        public Object GetOrDefault(Object key, Object defaultValue)
        {
            Object obj = Get(key);
            return obj == null ? defaultValue : obj;
        }

        /* Remove a given attribute from the property
         * If the property does not contain an element with the specified attribute, 
         * the property remains unchanged. No exception is thrown.
         */
        public bool RemoveProperty(String key)
        {
            return Remove(key);
        }

        /* Remove a given attribute from the property
      * If the property does not contain an element with the specified attribute, 
      * the property remains unchanged. No exception is thrown.
      */
        public bool Remove(Object key)
        {
            if (IsValidInput(key) && base.ContainsKey(key))
            {
                base.Remove(key);
                return true;
            }
            return false;
        }

        /* check if the inputs are valid
         */
        private bool IsValidInput(params Object[] args)
        {
            foreach (Object arg in args)
            {
                if (arg == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
