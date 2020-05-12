using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public interface IHasCommand
    {
        IEpsonCmd Cmd { get; }
    }

    public interface IHasDefault<T>
        where T : Enumeration<T>
    {
        T Default { get; }
    }

    public interface IHasUnknown<T>
        where T : Enumeration<T>
    {
        T Unknown { get; }
    }

    public interface IHasResponse
    {
        string SearchString { get; }
        string Response { get; }
    }

    public static class Extensions
    {
        public static string GetResponse<T>(this IHasResponse response)
            where T : IHasResponse
        {
            return response.Response;
        }
    }
}