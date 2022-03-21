using System;
using System.Collections.Generic;
using System.Linq;

static class StringExtensions
{
    public static T ToEnum<T>(this string enumString) where T : Enum
    {
        try
        {
            return (T)Enum.Parse(typeof(T), enumString, true);
        }
        catch (ArgumentException)
        {
            return default;
        }
    }
}

static class DictionaryExtensions
{
    public static V GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
    {
        var exists = dictionary.TryGetValue(key, out V value);
        return exists ? value : default;
    }

    public static V GetOrCreate<K, V>(this Dictionary<K, V> dictionary, K key, V newValue)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, newValue);
        }

        return dictionary[key];
    }
}

static class ListExtensions
{
    public static void Resize<T>(this List<T> list, int size, T element = default(T))
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            list.AddRange(Enumerable.Repeat(element, size - count));
        }
    }
}

static class ICollectionExtensions
{
    private static readonly Random Random = new Random();

    public static V GetRandom<V>(this ICollection<V> enumerable)
    {
        return enumerable.ElementAt(Random.Next(enumerable.Count()));
    }

    public static V PopRandom<V>(this ICollection<V> enumerable)
    {
        var randomValue = enumerable.ElementAt(Random.Next(enumerable.Count()));
        enumerable.Remove(randomValue);
        return randomValue;
    }
}
