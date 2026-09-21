// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.SharedKernel;

namespace Castle.Proxies;

public class CastleOrderEntity(Guid id) : Entity<Guid>(id);
