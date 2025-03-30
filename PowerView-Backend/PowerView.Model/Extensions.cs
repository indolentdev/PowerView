using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace PowerView.Model
{
    public static class Extensions
    {
        public static T Sum<T>(this IEnumerable<T> source)
        {
            return source.Aggregate((x, y) => (dynamic)x + (dynamic)y);
        }
    }

}

