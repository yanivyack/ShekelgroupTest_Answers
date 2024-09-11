namespace ENV.IO.Advanced.Internal
{
    interface TextControlReader:IStringToByteArrayConverter
    {
        string Read(int line, int position, int length, bool rightToLeft);
    }
}