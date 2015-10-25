namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api

open Org.Kevoree.Core.Api
open System.Collections.Generic

type PortOutput(name:string, path:string, componentz:ChannelDispatch, methodName:string) =
    inherit System.MarshalByRefObject()        

    interface Port with
        member this.send (payload:string, callback:Callback):unit = componentz.dispatch(payload, callback)
        member this.getPath ():string = path
        member this.getConnectedBindingsSize ():int = -1