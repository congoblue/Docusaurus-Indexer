Imports System
Imports System.IO

Public Class Form1

    Function LinkGen(link As String, entry As String, section As String) As String
        'encode the caption to be a link for md (spaces to hyphen and remove all other punctuation)
        Do While Strings.Right(entry, 1) = " "
            entry = Strings.Left(entry, Len(entry) - 1)
        Loop
        Dim encentry = LCase(Replace(entry, " ", "-"))
        encentry = Replace(encentry, ":", "")
        encentry = Replace(encentry, "(", "")
        encentry = Replace(encentry, ")", "")
        encentry = Replace(encentry, """", "")
        encentry = Replace(encentry, "/", "")
        encentry = Replace(encentry, "@", "")

        'make the actual link to show on the page
        'use the caption name, then add the section it is in, then the actual link
        LinkGen = "[" & entry & " *(in """ & section & """)*](." & link & "#" & encentry & ")"
        LinkGen = Replace(LinkGen, "\", "/")
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim entries() As String = {}
        Dim heading As String = ""

        'first we go through and extract all captions in the md files and subfolders, making links to them
        For Each filename As String In IO.Directory.GetFiles(TextBox1.Text, "*", IO.SearchOption.AllDirectories)

            Dim linktext = Replace(filename, TextBox1.Text, "")

            If linktext <> "\alpha-index.md" And InStr(linktext, "quick-start") = 0 Then 'do not index ourselves, or the quick-start section

                Dim Lines() As String = IO.File.ReadAllLines(filename)
                Dim i = 0
                For Each line As String In Lines
                    If InStr(line, "title: ") <> 0 Then
                        heading = Mid(line, 8) 'remember the title of this section so we can put it on the links
                    End If
                    If Strings.Left(line, 3) = "## " And InStr(line, "\<") = 0 Then 'look for h2 but miss out the button definitions in the reference section
                        Array.Resize(entries, entries.Length + 1) 'add to the end of the array
                        entries(entries.Length - 1) = LinkGen(linktext, Mid(line, 4), heading)
                        'heading = Mid(line, 4) 'uncomment this to update the title every time we pass a heading
                    End If
                    If Strings.Left(line, 4) = "### " Then 'look for h3
                        Array.Resize(entries, entries.Length + 1) 'add to the end of the array
                        entries(entries.Length - 1) = LinkGen(linktext, Mid(line, 5), heading)
                    End If
                    If i < UBound(Lines) Then 'look for h2 defined by underline
                        If Strings.Left(Lines(i + 1), 6) = "------" And InStr(line, "|") = 0 Then
                            Array.Resize(entries, entries.Length + 1) 'add to the end of the array
                            entries(entries.Length - 1) = LinkGen(linktext, line, heading)
                            'heading = line 'uncomment this to update the title every time we pass a heading
                        End If
                    End If
                    i = i + 1
                Next
            End If

        Next

        'then we organise them into alphabetical order with ### A..Z captions
        Array.Sort(entries)

        'For Each cap In entries
        'TextBox3.Text = TextBox3.Text & cap & vbCrLf
        'Next

        'then write out the file
        Dim alpha = "A"

        Dim file As New StreamWriter(TextBox2.Text)
        file.WriteLine("---")
        file.WriteLine("id: alpha-index") 'write the header
        file.WriteLine("title: Alphabetic Index")
        file.WriteLine("sidebar_label: Alphabetic Index")
        file.WriteLine("---")
        file.WriteLine(vbCrLf & "## " & alpha) 'write the first letter
        Dim c = 0
        For Each cap In entries
            If UCase(Mid(cap, 2, 1)) <> alpha Then 'if the first letter changes, write a new heading for it
                alpha = UCase(Mid(cap, 2, 1))
                file.WriteLine("  " & vbCrLf & "## " & alpha)
            End If

            If c < UBound(entries) Then
                If UCase(Mid(entries(c + 1), 2, 1)) = alpha Then
                    file.WriteLine(cap & "\") 'if we are continuing with the same letter, write the item with a line break character
                Else
                    file.WriteLine(cap) 'if a new letter, write the item but no line break
                End If
            Else
                file.WriteLine(cap) 'the very last line
            End If
            c = c + 1
        Next
        file.Close()

        MsgBox("Completed!")

    End Sub
End Class
