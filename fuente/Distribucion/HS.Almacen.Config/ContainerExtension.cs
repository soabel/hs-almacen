﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using HS.Almacen.Aplicacion.Servicios;
using HS.Almacen.Dominio.Entidades;
using HS.Almacen.Dominio.Eventos;
using HS.Almacen.Dominio.ManejadoresEventos;
using HS.Almacen.Dominio.Servicios;
using HS.Eventos;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HS.Almacen.Config
{
  public static class ContainerExtension
  {
    public static IWindsorContainer AlmacenStack(this IWindsorContainer container)
    {
      return container
        .AlmacenDominio()
        .AlmacenPersistencia()
        .AlmacenAplicacion()
        .AlmacenWebApi();
    }

    public static IWindsorContainer AlmacenDominio(this IWindsorContainer container)
    {
      return container
        .CoreDominio()
        .RegisterDependency<IInventarioFactory, InventarioFactory>()
        .RegisterDependency<IManejadorDeEvento<ArticuloIngresado>, CrearLotePorIngreso>()
        .RegisterDependency<IManejadorDeEvento<AntesGrabarEntidad<Articulo>>, ArticuloCrearCodigo>();
    }

    public static IWindsorContainer AlmacenPersistencia(this IWindsorContainer container)
    {
      return container
        .CorePersistencia()
        .Register(Component.For<ISessionFactory>().Instance(CrearSessionFactory()));
    }

    public static IWindsorContainer AlmacenAplicacion(this IWindsorContainer container)
    {
      return container
        .CoreAplicacion()
        .RegisterAppService<ICrudService<AlmacenDto>, CrudService<AlmacenDto, Dominio.Entidades.Almacen>>()
        .RegisterAppService<ICrudService<ArticuloDto>, CrudService<ArticuloDto, Dominio.Entidades.Articulo>>()
        .RegisterAppService<IInventarioService, InventarioService>();
    }

    public static IWindsorContainer AlmacenWebApi(this IWindsorContainer container)
    {
      var tipos = Assembly.Load("HS.Almacen.WebApi")
        .ExportedTypes.Where(c => c.EsHijo(typeof(ApiController))).ToArray();
      foreach (var tipo in tipos)
      {
        container.RegisterDependency(tipo);
      }
      return container;
    }

    public static ISessionFactory CrearSessionFactory()
    {
      var cfg = new Configuration();
      cfg.DataBaseIntegration(db =>
      {
        db.ConnectionStringName = "BDAlmacen";
        db.Dialect<MsSql2012Dialect>();
      });
      cfg.CurrentSessionContext<CallSessionContext>();

      var mapper = new ModelMapper();
      mapper.AddMappings(Assembly.Load("HS.Almacen.Persistencia.NHibernate").ExportedTypes.Where(c => c.Name.EndsWith("Mapa")).ToArray());
      var mappings = mapper.CompileMappingForAllExplicitlyAddedEntities();
      cfg.AddMapping(mappings);

      return cfg.BuildSessionFactory();
    }
  }
}
