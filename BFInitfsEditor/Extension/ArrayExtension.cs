namespace BFInitfsEditor.Extension
{
    public static class ArrayExtension
    {
        public static void Fill<T>(this T[] target, T with)
        {
            for (var i = 0; i < target.Length; i++)
                target[i] = with;
        }
    }
}