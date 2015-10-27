namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api

open Org.Kevoree.Core.Api
open System.Collections.Generic

type PortOutput(name:string, path:string) =
    inherit System.MarshalByRefObject()        

    let mutable channels:List<IComponentRunner> = new List<IComponentRunner>();

    member this.registerChannel(channel:IComponentRunner):unit = channels.Add(channel)    

    interface Port with
        member this.send (payload:string, callback:Callback):unit = 
            for chan in channels do
                chan.dispatch(payload, callback)
        member this.getPath ():string = path
        member this.getConnectedBindingsSize ():int = -1