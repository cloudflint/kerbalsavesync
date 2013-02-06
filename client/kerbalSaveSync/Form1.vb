﻿'    This program is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.

'    This program is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.

'    You should have received a copy of the GNU General Public License
'    along with this program.  If not, see <http://www.gnu.org/licenses/>.


Imports System.IO
Imports MySql.Data.MySqlClient

Public Class Form1
    'developed by J. Turner

    'ISSUES
    'Oh god where to begin
    'needs WAY more robust error handling
    'the way it reads/stores ships out the persitant file will fall down at the drop of a hat
    'This whole thing is in extreme alpha right now, if you trust this program with any data that you care about you are insane
    'The way it tracks what files are who's is kinda sucky
    'this whole thing is in general full of holes
    'alot of the console output needs to be formatted better
    'all the sql server connection info is curently hard coded in
    'need to add a mechanism to have vessels deleted off the server when they no longer exist in the owners save
    'Various outher issues that need documented!

    'BUGS
    'Trying to sync a save with no vessels in it causes crash
    'Various unhandled errors which could cause program to fail horribly, see warnings about possible null reference exceptions being thrown in the vb.net error list
    'various outher bugs that need documented!

    'BUILD NOTES
    'need to install mysql driver

    'CHANGELOG
    'v0.003 (dev)
    'started changelog!
    'Added code to allow deletion of players owned ships to be synced to server
    'fixed error where owned vessels where not being saved correctly

    'filepaths and stuff
    'config is saved in C:\Users\<username>\AppData\Local\kerbalSaveSync
    Dim mainsavelocation As String
    Dim ownedships()




    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        'handles locating the save file
        Dim fd As OpenFileDialog = New OpenFileDialog()


        fd.Title = "Open File Dialog"
        fd.InitialDirectory = "C:\"
        fd.Filter = "All files (*.*)|*.*|All files (*.*)|*.*"
        fd.FilterIndex = 2
        fd.RestoreDirectory = True

        If fd.ShowDialog() = DialogResult.OK Then
            mainsavelocation = fd.FileName
            savepath.Text = mainsavelocation
            My.Settings.savefile = mainsavelocation
        End If
        My.Settings.Save()

    End Sub

 

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load



        'splash stuff in console
        Console.WriteLine("Kerbal space program save sync program")
        Console.WriteLine("Developed by J. Turner 2013")
        Console.WriteLine("V0.004 (Dev)")
        Console.WriteLine("The soul of man has been given wings, and at last he is beginning to fly.")


        'load save path from settings if they have put it in before
        If My.Settings.savefile <> Nothing Then
            mainsavelocation = My.Settings.savefile
            savepath.Text = mainsavelocation
        End If

        'load owned ships
        If My.Settings.savefile <> Nothing Then
            Dim temp = My.Settings.ownedships
            ownedships = temp.Split(",")
        End If

    



    End Sub





    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click


        'sync order
        'download list of pid's from server
        'any pid thats in local save file and isnt on server - add to ownership list
        'export all ships to database
        'download ships from database
        'merge all ships into save file


        'begin variable declerations
        Dim servervessellist() ' holds list of pid's on server
        Dim servervessels()
        Dim i As Integer = 0 'general integer for loops 
        Dim x As Integer = 0 'general integer for loops
        Dim connStr As String = "server=" & "25.69.70.140" & ";" & "user id=" & "kspships" & ";" & "password=" & "zm5L7Jta5caSua2X" & ";" & "database=kspships"
        Dim mainsave As String = mainsavelocation '//Location of main save
        Dim savefiledata As String = IO.File.ReadAllText(mainsave) '// read file into a String.
        Dim temp As String
        Dim startindex As Integer = 0
        Dim endIndex As Integer = 0
        Dim found As Boolean = False
        Dim vessels() 'holds vessel data extracted from local save
        Dim vesselnames() 'holds list of vessesl names extracted from local save
        Dim startingtext As String

        'end variable decelerations

        Console.Write("Beginning save synchronisation")
        Console.Write(vbLf)
        Console.Write("A well oiled toaster oven") 'Where all my q3map2 modders at!
        Console.Write(vbLf)
        Console.Write("Reading list of vessesls from server")
        Console.Write(vbLf)
        Console.Write("Listing found ships from server:")
        Console.Write(vbLf)
        'begin read list of ships from server
        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand("SELECT pid FROM ships", conn)
                conn.Open()
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        'Console.WriteLine("{0}, {1}", reader.GetString(0), reader.GetString(1))

                        ReDim Preserve servervessellist(0 To i)
                        servervessellist(i) = reader.GetString(0)
                        Console.WriteLine(servervessellist(i))
                        i = i + 1
                    End While
                End Using
            End Using
        End Using
        i = 0
        'done reading list of ships from server

        Console.Write(vbLf)
        Console.Write(vbLf)
        Console.Write("Finished listing server ships, extracting ships from local save file")
        Console.Write(vbLf)
        Console.Write(vbLf)

        'Load  data fro mlocal save file into arrays


        Console.Write("Extracting vessels from save")
        Console.Write(vbLf)

        'extract the vessels from the persistance file
        While savefiledata.IndexOf(vbTab & vbTab & "VESSEL" & vbCrLf & vbTab & vbTab & "{", endIndex) <> -1
            startindex = savefiledata.IndexOf(vbTab & vbTab & "VESSEL" & vbCrLf & vbTab & vbTab & "{", endIndex) + 1
            endIndex = savefiledata.IndexOf(vbCrLf & vbTab & vbTab & "}" & vbCrLf, startindex) + 7
            ReDim Preserve vessels(0 To i)
            vessels(i) = savefiledata.Substring(startindex, endIndex - startindex).Trim
            i = i + 1
        End While
        i = 0

        Console.Write(vbLf)
        Console.Write(vbLf)
        Console.Write("Extracting vessel Pid's" & vbLf)
        'extract each vessels name

        For Each contents In vessels
            If contents <> Nothing Then
                If contents.IndexOf("pid = ") <> -1 Then
                    startindex = contents.IndexOf("pid = ") + 5
                    endIndex = contents.IndexOf(vbCrLf, startindex)
                    'MsgBox(startindex & "===" & endIndex)
                    ReDim Preserve vesselnames(0 To i)
                    vesselnames(i) = contents.Substring(startindex, endIndex - startindex).Trim
                    Console.Write(vesselnames(i) & vbLf)
                    i = i + 1
                End If
            End If
        Next contents
        i = 0





        Console.Write("Examing local ships against server ships: ")
        Console.Write(vbCrLf)
        Console.Write(vbCrLf)
        'ok, we loop through the vessesl in our save file now, if it is not found on the list of server ships take ownership of it
        'we should skip over anything that is already in teh owned list
        For Each temp In vesselnames


            found = False
            If servervessellist Is Nothing Then

            Else
                For Each value In servervessellist

                    If value = temp Then
                        found = True
                    End If

                Next value



            End If


            'if the ship we found in the local save file isnt found on the server, take owenrship of it.
            If found = False Then

                If ownedships Is Nothing Then
                    ReDim Preserve ownedships(0 To 0)
                    ownedships(0) = temp
                    My.Settings.ownedships = "," & ownedships(0)
                    My.Settings.Save()
                    Console.Write("Taking ownership of ship: " & temp & vbLf)
                    Console.WriteLine("first owned ship")
                Else
                    If ownedships.Contains(temp) Then
                        'do nothing
                    Else
                        ReDim Preserve ownedships(0 To ownedships.Length)
                        ownedships(ownedships.Length - 1) = temp
                        My.Settings.ownedships = My.Settings.ownedships & "," & ownedships(ownedships.Length - 1)
                        My.Settings.Save()
                        Console.Write("Taking ownership of ship: " & temp & vbLf)

                    End If
                End If





            End If


            i = i + 1


        Next
        i = 0



        Console.Write(vbCrLf)
        Console.Write("owened ships list is as follows:")
        Console.Write(vbCrLf)

        If ownedships Is Nothing Then
            'do nothing
        Else
            For Each temp In ownedships
                Console.Write(temp & vbLf)
            Next
        End If

        Console.Write(vbCrLf)
        Console.Write("Done listing owned ships")
        Console.Write(vbCrLf)










        'ok, we have the list of vessesl we own, now to download every ship but those from the server
        Console.Write("Uploading owned ships to server" & vbLf)
        'MsgBox("Now to write the files")
        'write all vessels to files
        If ownedships Is Nothing Then
            'if we have no ships we dont need to update anything!
        Else
            For Each contents In vesselnames

                If ownedships.Contains(contents) Then
                    Console.Write("Uploading ship: " & contents & vbLf)

                    'Insert/update ship into database
                    Using conn As New MySqlConnection(connStr)
                        Using cmd As New MySqlCommand("INSERT INTO ships(pid,content) VALUES ('" & vesselnames(i) & "','" & vessels(i) & "')  ON DUPLICATE KEY UPDATE content='" & vessels(i) & "';", conn)
                            conn.Open()
                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                'er...hello!
                            End Using
                        End Using
                    End Using
                    'done reading list of ships from server
                    'add ship to local list


                End If

                i = i + 1
            Next contents
            i = 0


        End If




        'read the new list of ships from the server
        Console.Write(vbCrLf)
        Console.Write("Checking list of ships on server after upload" & vbLf)
        'begin read list of ships from server
        'clear the old server list
        ReDim servervessellist(0 To 0)

        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand("SELECT pid FROM ships", conn)
                conn.Open()
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        'Console.WriteLine("{0}, {1}", reader.GetString(0), reader.GetString(1))

                        ReDim Preserve servervessellist(0 To i)
                        servervessellist(i) = reader.GetString(0)
                        Console.WriteLine("{0}", reader.GetString(0) & vbLf)
                        i = i + 1
                    End While
                End Using
            End Using
        End Using
        i = 0





        'remove owned ships which have been deleted
        'ok if a vessel is in servervessellist() and ownedships() but not in vesselnames(), that means we own it and have deleted it from our save, so remove it from the server
        Console.Write(vbLf)
        Console.Write(vbLf)
        Console.WriteLine("Sync deleted ships to server")

        For Each temp0 In ownedships 'loop through the list of vessels we own
            found = False
            Console.WriteLine("checking owned ship: " & temp0)
            For Each temp1 In vesselnames 'loop through the list of ships in our save file

                If temp0 Is Nothing Or temp1 Is Nothing Then

                Else
                    If temp0 = temp1 Then
                        found = True
                        Console.WriteLine(temp0 & " = " & temp1)

                    End If

                End If




            Next

            If found = True Then
                'ok we found the vessel in our owned ships list and its in our save, that means it exists so we can leave it!
            Else
                'ok, the ship exists in our list of owned ships but isnt in our save, that means it dosent exist, so lets see if its still on the server and if it is destroy it!

                If servervessellist.Contains(temp0) Then
                    'ok, we found the vessel in our list of ships on the server, so delete it!
                    Console.WriteLine("Deleting ship: " & temp0)
                    Using conn As New MySqlConnection(connStr)
                        Using cmd As New MySqlCommand("DELETE FROM ships WHERE pid='" & temp0 & "'", conn)
                            conn.Open()
                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                'look mum, no input!
                            End Using
                        End Using
                    End Using


                End If



            End If



        Next


        'read the new list of ships from the server
        Console.Write(vbCrLf)
        Console.Write("Checking list of ships on server after syncing deletions" & vbLf)
        'begin read list of ships from server
        'clear the old server list
        ReDim servervessellist(0 To 0)

        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand("SELECT pid FROM ships", conn)
                conn.Open()
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        'Console.WriteLine("{0}, {1}", reader.GetString(0), reader.GetString(1))

                        ReDim Preserve servervessellist(0 To i)
                        servervessellist(i) = reader.GetString(0)
                        Console.WriteLine("{0}", reader.GetString(0) & vbLf)
                        i = i + 1
                    End While
                End Using
            End Using
        End Using
        i = 0

        Console.Write("Downloading actual vessel data from server" & vbLf)
        For Each temp In servervessellist

            'ok, read all vessel data into array
            Console.Write("Downloading ship: " & temp & vbLf)
            'begin read list of ships from server

            Using conn As New MySqlConnection(connStr)
                Using cmd As New MySqlCommand("SELECT content FROM ships where pid='" & temp & "'", conn)
                    conn.Open()
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            'Console.WriteLine("{0}, {1}", reader.GetString(0), reader.GetString(1))

                            ReDim Preserve servervessels(0 To i)
                            servervessels(i) = reader.GetString(0)
                            'Console.WriteLine("{0}", reader.GetString(0))
                            i = i + 1
                        End While
                    End Using
                End Using
            End Using

        Next
        i = 0

        'ok, we have all the vessel data in, now to build the save file!
        Console.Write("Vessel data downloaded")
        Console.Write(vbLf)
        Console.Write("rebuilding persistant file" & vbLf)
        'get the text that goes in befroe the list of vessels >:D
        If savefiledata.IndexOf(vbTab & vbTab & "CREW") <> -1 Then
            endIndex = savefiledata.IndexOf(vbTab & vbTab & "CREW")
            startingtext = savefiledata.Substring(0, endIndex).Trim
            endIndex = 0
        End If

        My.Computer.FileSystem.WriteAllText(mainsavelocation, startingtext, False)


        'add in 3 crew for each vessel just to be safe?
        For Each contents In servervessels

            My.Computer.FileSystem.WriteAllText(mainsavelocation, _
            vbCrLf & vbTab & vbTab & "CREW" & vbCrLf _
            & vbTab & vbTab & "{" & vbCrLf _
            & vbTab & vbTab & vbTab & "name = Jebediah Kerman" & vbCrLf _
            & vbTab & vbTab & vbTab & "brave = 0.5" & vbCrLf _
            & vbTab & vbTab & vbTab & "dumb = 0.5" & vbCrLf _
            & vbTab & vbTab & vbTab & "badS = True" & vbCrLf _
            & vbTab & vbTab & vbTab & "state = 0" & vbCrLf _
            & vbTab & vbTab & vbTab & "ToD = 1" & vbCrLf _
            & vbTab & vbTab & vbTab & "idx = 0" & vbCrLf _
            & vbTab & vbTab & "}" & vbCrLf _
            , True)

            My.Computer.FileSystem.WriteAllText(mainsavelocation, _
           vbTab & vbTab & "CREW" & vbCrLf _
           & vbTab & vbTab & "{" & vbCrLf _
           & vbTab & vbTab & vbTab & "name = Jebediah Kerman" & vbCrLf _
           & vbTab & vbTab & vbTab & "brave = 0.5" & vbCrLf _
           & vbTab & vbTab & vbTab & "dumb = 0.5" & vbCrLf _
           & vbTab & vbTab & vbTab & "badS = True" & vbCrLf _
           & vbTab & vbTab & vbTab & "state = 0" & vbCrLf _
           & vbTab & vbTab & vbTab & "ToD = 1" & vbCrLf _
           & vbTab & vbTab & vbTab & "idx = 0" & vbCrLf _
           & vbTab & vbTab & "}" & vbCrLf _
           , True)

            My.Computer.FileSystem.WriteAllText(mainsavelocation, _
           vbTab & vbTab & "CREW" & vbCrLf _
           & vbTab & vbTab & "{" & vbCrLf _
           & vbTab & vbTab & vbTab & "name = Jebediah Kerman" & vbCrLf _
           & vbTab & vbTab & vbTab & "brave = 0.5" & vbCrLf _
           & vbTab & vbTab & vbTab & "dumb = 0.5" & vbCrLf _
           & vbTab & vbTab & vbTab & "badS = True" & vbCrLf _
           & vbTab & vbTab & vbTab & "state = 0" & vbCrLf _
           & vbTab & vbTab & vbTab & "ToD = 1" & vbCrLf _
           & vbTab & vbTab & vbTab & "idx = 0" & vbCrLf _
           & vbTab & vbTab & "}" _
           , True)


        Next


        For Each contents In servervessels
            If contents <> Nothing Then
                My.Computer.FileSystem.WriteAllText(mainsavelocation, vbCrLf & vbTab & vbTab & contents, True)
            End If
        Next

        My.Computer.FileSystem.WriteAllText(mainsavelocation, vbCrLf & vbTab & "}" & vbCrLf & "}" & vbCrLf, True)

        Console.Write("Finised writing persistant file")
        Console.Write(vbLf)
        Console.Write("Finsihed Sync!")
        Console.Write(vbLf)
        Console.Write("Gods in his heaven alls right with the world") 'i'd like to say this was a Robert Browning reference, but im not that sophisticated

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        If mainsavelocation Is Nothing Then
        Else
            Button5.Enabled = True
        End If



    End Sub
End Class


