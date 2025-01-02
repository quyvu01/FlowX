using FlowX.EntityFrameworkCore.Abstractions;

namespace FlowX.Tests.Models;

public class User : IEfModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}