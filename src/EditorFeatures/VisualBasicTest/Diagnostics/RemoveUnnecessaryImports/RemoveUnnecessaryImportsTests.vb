' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Option Strict Off
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Extensions
Imports Microsoft.CodeAnalysis.VisualBasic.CodeFixes.RemoveUnnecessaryImports
Imports Microsoft.CodeAnalysis.VisualBasic.Diagnostics.RemoveUnnecessaryImports

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Diagnostics.RemoveUnnecessaryImports
    Partial Public Class RemoveUnnecessaryImportsTests
        Inherits AbstractVisualBasicDiagnosticProviderBasedUserDiagnosticTest

        Friend Overrides Function CreateDiagnosticProviderAndFixer(workspace As Workspace) As Tuple(Of DiagnosticAnalyzer, CodeFixProvider)
            Return New Tuple(Of DiagnosticAnalyzer, CodeFixProvider)(New VisualBasicRemoveUnnecessaryImportsDiagnosticAnalyzer(), New RemoveUnnecessaryImportsCodeFixProvider())
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestProjectLevelMemberImport1() As Task
            Await TestAsync(
"[|Imports System
Module Program
    Sub Main(args As DateTime())
    End Sub
End Module|]",
"Module Program
    Sub Main(args As DateTime())
    End Sub
End Module",
parseOptions:=TestOptions.Regular,
compilationOptions:=TestOptions.ReleaseExe.WithGlobalImports({GlobalImport.Parse("System")}))
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestProjectLevelMemberImport2() As Task
            Await TestMissingAsync(
"[|Imports System
Module Program
    Sub Main(args As DateTime())
    End Sub
End Module|]")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestNoImports() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
    End Sub
End Module|]",
"Module Program
    Sub Main(args As String())
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestSimpleTypeName() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
        Dim s As DateTime
    End Sub
End Module|]",
"Imports System
Module Program
    Sub Main(args As String())
        Dim s As DateTime
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestGenericTypeName() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
        Dim s As List(Of Integer)
    End Sub
End Module|]",
"Imports System.Collections.Generic
Module Program
    Sub Main(args As String())
        Dim s As List(Of Integer)
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestNamespaceName() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
        Dim s As Collections.Generic.List(Of Integer)
    End Sub
End Module|]",
"Imports System
Module Program
    Sub Main(args As String())
        Dim s As Collections.Generic.List(Of Integer)
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestAliasName() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports G = System.Collections.Generic
Module Program
    Sub Main(args As String())
        Dim s As G.List(Of Integer)
    End Sub
End Module|]",
"Imports G = System.Collections.Generic
Module Program
    Sub Main(args As String())
        Dim s As G.List(Of Integer)
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestExtensionMethod() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
        args.Where(Function(a) a.Length > 21)
    End Sub
End Module|]",
"Imports System.Linq
Module Program
    Sub Main(args As String())
        args.Where(Function(a) a.Length > 21)
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestModuleMember() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Foo
Namespace Foo
    Public Module M
        Public Sub Bar(i As Integer)
        End Sub
    End Module
End Namespace
Module Program
    Sub Main(args As String())
        Bar(0)
    End Sub
End Module|]",
"Imports Foo
Namespace Foo
    Public Module M
        Public Sub Bar(i As Integer)
        End Sub
    End Module
End Namespace
Module Program
    Sub Main(args As String())
        Bar(0)
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestInvalidCodeRemovesImports() As Task
            Await TestAsync(
"[|Imports System
Imports System.Collections.Generic
Module Program
    Sub Main()
        gibberish Dim lst As List(Of String) 
 Console.WriteLine(""TEST"")
    End Sub
End Module|]",
"Imports System
Module Program
    Sub Main()
        gibberish Dim lst As List(Of String) 
 Console.WriteLine(""TEST"")
    End Sub
End Module")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestExcludedCodeIsIgnored() As Task
            Await TestAsync(
"[|Imports System
Module Program
    Sub Main()
#If False Then
 Console.WriteLine(""TEST"") 
#End If
    End Sub
End Module|]",
"Module Program
    Sub Main()
#If False Then
 Console.WriteLine(""TEST"") 
#End If
    End Sub
End Module")
        End Function

        <WorkItem(541744, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541744")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestCommentsAroundImportsStatement() As Task
            Await TestAsync(
<Text>'c1
[|Imports System.Configuration 'c2
Imports System, System.Collections.Generic 'c3
'c4

Module Module1
    Sub Main()
        Dim x As List(Of Integer)
    End Sub
End Module|]</Text>.NormalizedValue,
<Text>'c1
Imports System.Collections.Generic 'c3
'c4

Module Module1
    Sub Main()
        Dim x As List(Of Integer)
    End Sub
End Module</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(541747, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541747")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestAttribute() As Task
            Await TestMissingAsync(
"[|Imports SomeNamespace
<SomeAttr>
Class Foo
End Class
Namespace SomeNamespace
    Public Class SomeAttrAttribute
        Inherits Attribute
    End Class
End Namespace|]")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestAttributeArgument() As Task
            Await TestMissingAsync(
"[|Imports System
Imports SomeNamespace
<SomeAttribute(Foo.C)>
Module Program
    Sub Main(args As String())
    End Sub
End Module
Namespace SomeNamespace
    Public Enum Foo
        A
        B
        C
    End Enum
End Namespace
Public Class SomeAttribute
    Inherits Attribute
    Public Sub New(x As SomeNamespace.Foo)
    End Sub
End Class|]")
        End Function

        <WorkItem(541757, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541757")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsSurroundedByDirectives() As Task
            Await TestAsync(
"#If True Then
[|Imports System.Collections.Generic
#End If
Module Program
End Module|]",
"#If True Then
#End If
Module Program
End Module")
        End Function

        <WorkItem(541758, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541758")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemovingUnbindableImports() As Task
            Await TestAsync(
"[|Imports gibberish
Module Program
End Module|]",
"Module Program
End Module")
        End Function

        <WorkItem(541744, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541744")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestPreservePrecedingComments() As Task
            Await TestAsync(
<Text>' c1
[|Imports System 'c2
' C3

Module Module1
End Module|]</Text>.NormalizedValue,
<Text>' c1
' C3

Module Module1
End Module</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(541757, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541757")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestDirective1() As Task
            Await TestAsync(
<Text>#If True Then
[|Imports System.Collections.Generic
#End If

Module Program
End Module|]</Text>.NormalizedValue,
<Text>#If True Then
#End If

Module Program
End Module</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(541757, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541757")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestDirective2() As Task
            Await TestAsync(
<Text>#If True Then
[|Imports System
Imports System.Collections.Generic
#End If

Module Program
    Dim a As List(Of Integer)
End Module|]</Text>.NormalizedValue,
<Text>#If True Then
Imports System.Collections.Generic
#End If

Module Program
    Dim a As List(Of Integer)
End Module</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(541932, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541932")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsClauseRemoval1() As Task
            Await TestAsync(
"[|Imports System, foo, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace|]",
"Imports System, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace")
        End Function

        <WorkItem(541932, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541932")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsClauseRemoval2() As Task
            Await TestAsync(
"[|Imports System, System.Collections.Generic, foo
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace|]",
"Imports System, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace")
        End Function

        <WorkItem(541932, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541932")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsClauseRemoval3() As Task
            Await TestAsync(
"[|Imports foo, System, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace|]",
"Imports System, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace")
        End Function

        <WorkItem(541758, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541758")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestUnbindableNamespace() As Task
            Await TestAsync(
"[|Imports gibberish
Module Program
End Module|]",
"Module Program
End Module")
        End Function

        <WorkItem(541780, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541780")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveClause() As Task
            Await TestAsync(
"[|Imports System, foo, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace|]",
"Imports System, System.Collections.Generic
Module Program
    Sub Main(args As String())
        Console.WriteLine(""TEST"")
        Dim q As List(Of Integer)
    End Sub
End Module
Namespace foo
    Class bar
    End Class
End Namespace")
        End Function

        <WorkItem(528603, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528603")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveClauseWithExplicitLC1() As Task
            Await TestAsync(
"[|Imports A _
 , B
Module Program
    Sub Main(args As String())
        Dim q As CA
    End Sub
End Module
Namespace A
    Public Class CA
    End Class
End Namespace
Namespace B
    Public Class CB
    End Class
End Namespace|]",
"Imports A
Module Program
    Sub Main(args As String())
        Dim q As CA
    End Sub
End Module
Namespace A
    Public Class CA
    End Class
End Namespace
Namespace B
    Public Class CB
    End Class
End Namespace")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveClauseWithExplicitLC2() As Task
            Await TestAsync(
"[|Imports B _
 , A
Module Program
    Sub Main(args As String())
        Dim q As CA
    End Sub
End Module
Namespace A
    Public Class CA
    End Class
End Namespace
Namespace B
    Public Class CB
    End Class
End Namespace|]",
"Imports A
Module Program
    Sub Main(args As String())
        Dim q As CA
    End Sub
End Module
Namespace A
    Public Class CA
    End Class
End Namespace
Namespace B
    Public Class CB
    End Class
End Namespace")
        End Function

        <WorkItem(528603, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528603")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveClauseWithExplicitLC3() As Task
            Await TestAsync(
<Text>[|Imports A _
    , B _
    , C

Module Program
    Sub Main()
        Dim q As CB

    End Sub
End Module

Namespace A
    Public Class CA
    End Class
End Namespace

Namespace B
    Public Class CB
    End Class
End Namespace

Namespace C
    Public Class CC
    End Class
End Namespace|]</Text>.NormalizedValue,
<Text>Imports B _

Module Program
    Sub Main()
        Dim q As CB

    End Sub
End Module

Namespace A
    Public Class CA
    End Class
End Namespace

Namespace B
    Public Class CB
    End Class
End Namespace

Namespace C
    Public Class CC
    End Class
End Namespace</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestTypeImports() As Task
            Await TestAsync(
<Text>[|Imports Foo

Module Program
    Sub Main()
    End Sub
End Module

Public Class Foo
    Shared Sub Bar()
    End Sub
End Class|]</Text>.NormalizedValue,
<Text>Module Program
    Sub Main()
    End Sub
End Module

Public Class Foo
    Shared Sub Bar()
    End Sub
End Class</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(528603, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528603")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestTypeImports_DoesNotRemove() As Task
            Await TestMissingAsync(
<Text>[|Imports Foo

Module Program
    Sub Main()
        Bar()
    End Sub
End Module

Public Class Foo
    Shared Sub Bar()

    End Sub
End Class|]</Text>.NormalizedValue, parseOptions:=Nothing)
            ' TODO: Enable testing in script when it comes online
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestAlias() As Task
            Await TestAsync(
<Text>[|Imports F = SomeNS

Module Program
    Sub Main()
    End Sub
End Module

Namespace SomeNS
    Public Class Foo
    End Class
End Namespace|]</Text>.NormalizedValue,
<Text>Module Program
    Sub Main()
    End Sub
End Module

Namespace SomeNS
    Public Class Foo
    End Class
End Namespace</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestAlias_DoesNotRemove() As Task
            Await TestMissingAsync(
<Text>[|Imports F = SomeNS

Module Program
    Sub Main()
        Dim q As F.Foo
    End Sub
End Module

Namespace SomeNS
    Public Class Foo
    End Class
End Namespace|]</Text>.NormalizedValue)
        End Function

        <WorkItem(541809, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541809")>
        <WorkItem(16488, "DevDiv_Projects/Roslyn")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsOnSameLine1() As Task
            Await TestAsync(
<Text>[|Imports A : Imports B

Module Program
    Sub Main()
        Dim q As ClassA
    End Sub
End Module

Namespace A
    Public Class ClassA
    End Class
End Namespace

Namespace B
    Public Class ClassB
    End Class
End Namespace|]</Text>.NormalizedValue,
<Text>Imports A

Module Program
    Sub Main()
        Dim q As ClassA
    End Sub
End Module

Namespace A
    Public Class ClassA
    End Class
End Namespace

Namespace B
    Public Class ClassB
    End Class
End Namespace</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportsOnSameLine2() As Task
            Await TestAsync(
<Text>[|Imports A : Imports B
Imports C

Module Program
    Sub Main()
        Dim q1 As ClassA
        Dim q2 As ClassC
    End Sub
End Module

Namespace A
    Public Class ClassA
    End Class
End Namespace

Namespace B
    Public Class ClassB
    End Class
End Namespace

Namespace C
    Public Class ClassC
    End Class
End Namespace|]</Text>.NormalizedValue,
<Text>Imports A
Imports C

Module Program
    Sub Main()
        Dim q1 As ClassA
        Dim q2 As ClassC
    End Sub
End Module

Namespace A
    Public Class ClassA
    End Class
End Namespace

Namespace B
    Public Class ClassB
    End Class
End Namespace

Namespace C
    Public Class ClassC
    End Class
End Namespace</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(541808, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541808")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestTypeImport1() As Task
            Await TestMissingAsync(
"[|Imports Foo
Module Program
    Sub Main()
        Bar()
    End Sub
End Module
Public Class Foo
    Shared Sub Bar()
    End Sub
End Class|]",
parseOptions:=Nothing) 'TODO (tomat): modules not yet supported in script
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestTypeImport2() As Task
            Await TestMissingAsync(
"[|Imports Foo
Module Program
    Sub Main()
        Dim q As Integer = Bar
    End Sub
End Module
Public Class Foo
    Public Shared Bar As Integer
    End Sub
End Class|]",
parseOptions:=Nothing) 'TODO (tomat): modules not yet supported in script
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestUnusedTypeImportIsRemoved() As Task
            Await TestAsync(
<Text>[|Imports SomeNS.Foo

Module Program
    Sub Main(args As String())
    End Sub
End Module

Namespace SomeNS
    Module Foo
    End Module
End Namespace|]</Text>.NormalizedValue,
<Text>Module Program
    Sub Main(args As String())
    End Sub
End Module

Namespace SomeNS
    Module Foo
    End Module
End Namespace</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(528643, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528643")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestExtensionMethodLinq() As Task
            ' TODO: Enable script context testing.

            Await TestMissingAsync(<Text>[|Imports System.Collections
Imports System
Imports SomeNS

Public Module Program
    Sub Main()
        Dim qq As Foo = New Foo()
        Dim x As IEnumerable = From q In qq Select q
    End Sub
End Module

Public Class Foo
    Public Sub Foo()
    End Sub
End Class

Namespace SomeNS
    Public Module SomeClass
        &lt;System.Runtime.CompilerServices.ExtensionAttribute()&gt;
        Public Function [Select](ByRef o As Foo, f As Func(Of Object, Object)) As IEnumerable
            Return Nothing
        End Function
    End Module
End Namespace|]</Text>.NormalizedValue, parseOptions:=TestOptions.Regular)
        End Function

        <WorkItem(543217, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543217")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestExtensionMethodLinq2() As Task
            Await TestMissingAsync(
<Text>[|Imports System.Collections.Generic
Imports System.Linq
 
Module Program
    Sub Main(args As String())
        Dim product = New With {Key .Name = "", Key .Price = 0}
        Dim products = ToList(product)
        Dim namePriceQuery = From prod In products
                             Select prod.Name, prod.Price
    End Sub
 
    Function ToList(Of T)(a As T) As IEnumerable(Of T)
        Return Nothing
    End Function
End Module
|]</Text>.NormalizedValue)
        End Function

        <WorkItem(542135, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542135")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImportedTypeUsedAsGenericTypeArgument() As Task
            Await TestMissingAsync(
<Text>[|Imports GenericThingie

Public Class GenericType(Of T)
End Class

Namespace GenericThingie

    Public Class Something
    End Class

End Namespace

Public Class Program
    Sub foo()
        Dim type As GenericType(Of Something)
    End Sub
End Class|]</Text>.NormalizedValue)
        End Function

        <WorkItem(542132, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542132")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveSuperfluousNewLines1() As Task
            Await TestAsync(
<Text>[|Imports System
Imports System.Collections.Generic
Imports System.Linq

Module Program
    Sub Main(args As String())

    End Sub
End Module|]</Text>.NormalizedValue,
<Text>Module Program
    Sub Main(args As String())

    End Sub
End Module</Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(542132, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542132")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemoveSuperfluousNewLines2() As Task
            Await TestAsync(
<Text><![CDATA[[|Imports System
Imports System.Collections.Generic
Imports System.Linq


<Assembly: System.Obsolete()>


Module Program
    Sub Main(args As String())

    End Sub
End Module|]]]></Text>.NormalizedValue,
<Text><![CDATA[<Assembly: System.Obsolete()>


Module Program
    Sub Main(args As String())

    End Sub
End Module]]></Text>.NormalizedValue,
compareTokens:=False)
        End Function

        <WorkItem(542895, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542895")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRegressionFor10326() As Task
            Await TestAsync(
"[|Imports System.ComponentModel
<Foo(GetType(Category))>
Class Category
End Class|]",
"<Foo(GetType(Category))>
Class Category
End Class")
        End Function

        <WorkItem(712656, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/712656")>
        <Fact(), Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemovalSpan1() As Task
            Await TestSpansAsync(
<text>    [|Imports System

Namespace N
End Namespace|]</text>.NormalizedValue,
<text>    [|Imports System|]

Namespace N
End Namespace</text>.NormalizedValue)
        End Function

        <WorkItem(545434, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545434")>
        <WorkItem(712656, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/712656")>
        <Fact(), Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemovalSpan2() As Task
            Await TestSpansAsync(
<text>
#Const A = 1
[|Imports System
#Const B = 1
Imports System.Runtime.InteropServices|]</text>.NormalizedValue,
<text>
#Const A = 1
[|Imports System|]
#Const B = 1
[|Imports System.Runtime.InteropServices|]</text>.NormalizedValue, diagnosticId:=IDEDiagnosticIds.RemoveUnnecessaryImportsDiagnosticId)
        End Function

        <WorkItem(712656, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/712656")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestRemovalSpan3() As Task
            Await TestAsync(
<text>
#Const A = 1
Imports System
[|#Const B = 1|]
Imports System.Runtime.InteropServices
Class C : Dim x As Action : End Class</text>.NormalizedValue,
<text>
#Const A = 1
Imports System
#Const B = 1
Class C : Dim x As Action : End Class</text>.NormalizedValue)
        End Function

        <WorkItem(545831, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545831")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestImplicitElementAtOrDefault() As Task
            Await TestAsync(
<Text><![CDATA[[|Option Strict On

Imports System
Imports System.Text
Imports System.Runtime.CompilerServices
Imports N

Module M
    Sub Main()
        Foo(Function(x) x(0), Nothing)
    End Sub

    Sub Foo(x As Func(Of C, Object), y As String)
        Console.WriteLine(1)
    End Sub

    Sub Foo(x As Func(Of Integer(), Object), y As Object)
        Console.WriteLine(2)
    End Sub
End Module

Class C
    Public Function ElementAtOrDefault(index As Integer) As Integer
    End Function
End Class

Namespace N
    Module E
        <Extension>
        Function [Select](x As C, y As Func(Of Integer, Integer)) As C
        End Function
    End Module
End Namespace|]]]></Text>.NormalizedValue,
<Text><![CDATA[Option Strict On

Imports System
Imports System.Runtime.CompilerServices
Imports N

Module M
    Sub Main()
        Foo(Function(x) x(0), Nothing)
    End Sub

    Sub Foo(x As Func(Of C, Object), y As String)
        Console.WriteLine(1)
    End Sub

    Sub Foo(x As Func(Of Integer(), Object), y As Object)
        Console.WriteLine(2)
    End Sub
End Module

Class C
    Public Function ElementAtOrDefault(index As Integer) As Integer
    End Function
End Class

Namespace N
    Module E
        <Extension>
        Function [Select](x As C, y As Func(Of Integer, Integer)) As C
        End Function
    End Module
End Namespace]]></Text>.NormalizedValue)
        End Function

        <WorkItem(545964, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545964")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)>
        Public Async Function TestMissingOnSynthesizedEventType() As Task
            Await TestMissingAsync(
"[|Class C
    Event E()
End Class|]")
        End Function
    End Class
End Namespace
