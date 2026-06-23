namespace ParLibrary.Sllz;

/// <summary>
/// SLLZ compression exception.
/// </summary>
public class SllzCompressorException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="SllzCompressorException"/> class.
    /// </summary>
    public SllzCompressorException()
        : base() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SllzCompressorException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public SllzCompressorException(string message)
        : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SllzCompressorException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception.</param>
    public SllzCompressorException(string message, Exception innerException)
        : base(message, innerException) { }
}
