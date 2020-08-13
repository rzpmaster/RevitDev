namespace Log4NetDemo.Appender.ErrorHandler
{
    public enum ErrorCode : int
    {
        /// <summary>
        /// A general error
        /// </summary>
        GenericFailure = 0,

        /// <summary>
        /// Error while writing output
        /// </summary>
        WriteFailure,

        /// <summary>
        /// Failed to flush file
        /// </summary>
        FlushFailure,

        /// <summary>
        /// Failed to close file
        /// </summary>
        CloseFailure,

        /// <summary>
        /// Unable to open output file
        /// </summary>
        FileOpenFailure,

        /// <summary>
        /// No layout specified
        /// </summary>
        MissingLayout,

        /// <summary>
        /// Failed to parse address
        /// </summary>
        AddressParseFailure
    }
}
