﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HS.Almacen.Dominio.Entidades
{
  public class Articulo: EntityBase
  {
    public const string KeySecuencia = "SEQ-ARTICULO";

    public virtual string Codigo { get; set; }
    public virtual string Descripcion { get; set; }
  }
}
