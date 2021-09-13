using System;
using System.Collections;

public interface IContentLoader<T>
{
    IEnumerator LoadContent(T item);
    void LoadContentAsync(T item, Action<bool> action);
    void UpdateContent(T item);
}
