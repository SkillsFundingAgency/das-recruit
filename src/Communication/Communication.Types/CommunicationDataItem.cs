namespace Communication.Types
{
    public struct CommunicationDataItem
    {
        public string Key { get; }
        public string Value { get; }

        public CommunicationDataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
