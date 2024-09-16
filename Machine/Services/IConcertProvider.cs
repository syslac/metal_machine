using System;
using MetalMachine.Models;

namespace MetalMachine.Services;

public enum RetrySuggestion 
{
    Retry,
    WaitAndRetry,
    Stop,
    Good
}
public interface IConcertProvider
{
    public void Init(string initData);

    public Concert? GetNextConcert();

    public Task<RetrySuggestion> PopulateConcertList(string? extraParameters = null);

    public void ResetSeek();
}
