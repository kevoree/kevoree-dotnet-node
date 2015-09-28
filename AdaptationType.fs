﻿namespace Kevoree.Api

module AdaptationType =

    ///[<System.FlagsAttribute>]
    type AdaptationType =
        | AddDeployUnit = 1
        | RemoveDeployUnit = 2
        | UpdateInstance =3
        | UpdateBinding = 4
        | UpdateDictionaryInstance =5
        | AddInstance = 6
        | RemoveInstance = 7
        | AddBinding = 8
        | RemoveBinding = 9
        | StartInstance = 10
        | StopInstance = 11
        | LinkDeployUnit = 12
        | UpdateCallMethod = 13
        | UpgradeInstance = 14

    [<StructuredFormatDisplay("Adaptation({Type}, {PrimitiveType}, {Ref})"); CustomEquality; CustomComparison>]
    type Adaptation  =
        {   Type: AdaptationType;
            NodePath: string
            Ref: obj }
        override this.Equals(yobj) =
            match yobj with
                | :? Adaptation as y  -> y.Type = this.Type && y.Ref = this.Ref
                | _ -> false
        override this.GetHashCode() = hash (this.Type.GetHashCode(), this.NodePath.GetHashCode(), this.Ref.GetHashCode())
        interface System.IComparable with
            member this.CompareTo  yobj =
                match yobj with
                | :? Adaptation as y -> compare (this.Type, this.NodePath) (y.Type, y.NodePath)
                | _ -> invalidArg "yobj" "cannot compare values of different types"


    type AdaptationModel = Set<Adaptation>

    type ModelRegistry = Map<string,obj>