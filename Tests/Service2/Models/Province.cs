using FlowX.EntityFrameworkCore.Abstractions;

namespace Service2.Models;

public class Province : IEfModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}