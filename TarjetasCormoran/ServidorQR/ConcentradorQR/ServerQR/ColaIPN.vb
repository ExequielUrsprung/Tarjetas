Public NotInheritable Class ColaIPN
    Private Shared ReadOnly _instance As New Lazy(Of ColaIPN)(Function() New ColaIPN(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication)
    Dim waiting_queue As Queue
    Dim procc_list As ArrayList
    Dim locker As Object

    Private Sub New()
        waiting_queue = New Queue()
        locker = New Object()
        procc_list = New ArrayList

    End Sub

    Public Shared ReadOnly Property Instance() As ColaIPN
        Get
            Return _instance.Value
        End Get
    End Property

    Public Sub add_to_waiting_queue(filename As String)
        If waiting_queue.Contains(filename) OrElse procc_list.Contains(filename) Then
            Exit Sub
        End If

        waiting_queue.Enqueue(filename)
    End Sub

    Public Function take_from_waiting_queue() As String
        Dim selected As String

        SyncLock locker
            selected = waiting_queue.Dequeue()
        End SyncLock

        procc_list.Add(selected)

        Return selected
    End Function

    Public Sub final_procc(filename As String)
        procc_list.Remove(filename)
    End Sub


End Class
