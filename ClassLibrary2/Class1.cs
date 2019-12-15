namespace ClassLibrary2
{
    public static class Class1
    {
        public static int SharedVariable = 0;

        public static void Increment(int n)
        {
            for (var i = 0; i < n; i++)
                SharedVariable++;
        }
    }
}
