using System;
using System.Text;
using ENV.Utilities;

namespace ENV.IO.Advanced
{

    interface TokenConsumer
    {
        void Token(string token);
        void Text(string text);
        void Done();
    }
    class TokenParser
    {
        TokenConsumer _consumer;
        char[] _prefix;
        char[] _suffix;
        

        public TokenParser(TokenConsumer consumer, string prefix, string suffix)
        {
            _consumer = consumer;
            _prefix = prefix.ToCharArray();
            
            _suffix = suffix.ToCharArray();
        }

        public void Process(System.IO.TextReader source)
        {
            new StringParser().Parse(source, TextProcessor());
            _consumer.Done();
            source.Dispose();
        }
        
        CharProcessor TextProcessor()
        {
            return new ReadUntilChars(TokenProcessor, _prefix, _consumer.Text);
        }
        CharProcessor TokenProcessor()
        {
            return new ReadUntilChars(TextProcessor, _suffix, _consumer.Token);
        }
    }
    


}