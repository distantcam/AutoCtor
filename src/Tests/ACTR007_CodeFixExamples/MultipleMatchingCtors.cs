public class MultipleMatchingCtors
{
    private readonly string _str;
    private readonly int _num;

    public MultipleMatchingCtors(string str, int num)
    {
        _str = str;
        _num = num;
    }

    public MultipleMatchingCtors(int num, string str)
    {
        _num = num;
        _str = str;
    }
}
