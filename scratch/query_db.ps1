$connString = "Server=tcp:servidorsqljavidev.database.windows.net,1433;Initial Catalog=BDPruebasSNIER;User ID=adminsql;Password=Javiereg32;Encrypt=True;TrustServerCertificate=False;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT IdUsuario, Correo, Nombre, Cargo FROM dgmesnie.Usuario WHERE Vigente = 1"
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$adapter.Fill($dt) | Out-Null
$dt | Format-Table
$conn.Close()
