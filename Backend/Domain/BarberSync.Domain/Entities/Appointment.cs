using System;
using BarberSync.Domain.Common;

namespace BarberSync.Domain.Entities;
public class Appointment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
