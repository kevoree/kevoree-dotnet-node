namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

(*
    Does not perform any action
*)
type NullCommand() =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = true
        member this.Undo() = ()