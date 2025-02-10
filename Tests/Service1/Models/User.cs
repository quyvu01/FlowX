using FlowX.EntityFrameworkCore.Abstractions;

namespace Service1.Models;

public class User : IEfModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}