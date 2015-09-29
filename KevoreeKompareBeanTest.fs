namespace Org.Kevoree.Library

module Test =
    open NUnit.Framework
    open org.kevoree.factory;
    open Org.Kevoree.Library.AdaptationType
    open Org.Kevoree.Library.KevoreeKompareBean
    open org.kevoree;
    open java.io;
    open org.kevoree.modeling.api.trace;

    let rec prettyPrintB: List<Adaptation> -> AdaptationType -> Unit = fun adapt tp ->
        match adapt with
        | [] -> ()
        | x :: xs -> 
            if x.Type = tp
            then
                System.Diagnostics.Debug.WriteLine ("\tNodePath : " + x.NodePath)
                prettyPrintB xs tp
            else 
                System.Diagnostics.Debug.WriteLine ("}")
                prettyPrint adapt


    and prettyPrint:List<Adaptation> -> Unit = fun adapt ->
        match adapt with
        | [] -> ()
        | x :: xs -> 
            let tp = x.Type
            System.Diagnostics.Debug.WriteLine ("Primitive type { " + tp.ToString())
            prettyPrintB adapt tp

            
        

    [<TestFixture>]
    type KevoreeKompareBeanTest() = 

        [<Test>]
        member this.Mock() = 
            let factory = DefaultKevoreeFactory()
            let file1:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state1.json")).get(0) :?> ContainerRoot;
            let file2:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state2.json")).get(0) :?> ContainerRoot;
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node381"
            let traces = modelCompare.diff(file1, file2)
            let result = plan file1  file2  nodeName traces
            let expected = []
            CollectionAssert.IsEmpty([])

        [<Test>]
        member this.StopInstance() = 
            let factory = DefaultKevoreeFactory()
            let file1:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state2.json")).get(0) :?> ContainerRoot;
            let file2:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state1.json")).get(0) :?> ContainerRoot;
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node381"
            let traces = modelCompare.diff(file1, file2)
            let result = plan file1  file2  nodeName traces
            let expected = []
            prettyPrint (Set.toList result)
            ()

        [<Test>]
        member this.LinkInstance() = 
            let factory = DefaultKevoreeFactory()
            let file1:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\notlinked.json")).get(0) :?> ContainerRoot;
            let file2:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\linked.json")).get(0) :?> ContainerRoot;
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node381"
            let traces = modelCompare.diff(file1, file2)
            let result = plan file1  file2  nodeName traces
            let expected = []
            prettyPrint (Set.toList result)
            ()

        [<Test>]
        member this.InitFromNull() =
            let factory = DefaultKevoreeFactory()
            let file1:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state1.json")).get(0) :?> ContainerRoot;
            let file2:ContainerRoot = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\state1bis.json")).get(0) :?> ContainerRoot;
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node381"
            let traces = modelCompare.diff(file1, file2)
            let result = plan file1  file2  nodeName traces
            let expected = []
            prettyPrint (Set.toList result)
            CollectionAssert.IsEmpty([])