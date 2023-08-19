using System;
using System.Collections.Generic;

public static class CollectionExtensions
{
    private static Random _rand = new Random();

    public static T RandomElement<T>(this IList<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }
        if (list.Count == 0)
        {
            throw new InvalidOperationException("The collection is empty.");
        }

        return list[_rand.Next(list.Count)];
    }


    public static T PopRandomElement<T>(this IList<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }
        if (list.Count == 0)
        {
            throw new InvalidOperationException("The collection is empty.");
        }

        int randomIndex = _rand.Next(list.Count);
        T selectedElement = list[randomIndex];
        list.RemoveAt(randomIndex);
        return selectedElement;
    }
}
