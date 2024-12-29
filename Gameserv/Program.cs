using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        const int port = 5000; // Порт для подключения
        var listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Server started on port {port}. Waiting for connections...");

            while (true)
            {
                var client = listener.AcceptTcpClient(); // Ожидание подключения клиента
                Console.WriteLine("Client connected!");

                // Обработка клиента в отдельном потоке
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    private static void HandleClient(object clientObj)
    {
        if (clientObj is not TcpClient client)
            return;

        try
        {
            using (var stream = client.GetStream())
            {
                StringBuilder messageBuffer = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Преобразование байтов в строку
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Добавление к общему буферу
                    messageBuffer.Append(received);

                    // Проверяем, содержит ли буфер символ конца строки
                    if (messageBuffer.ToString().Contains("\n"))
                    {
                        // Полученное сообщение
                        string completeMessage = messageBuffer.ToString().Trim();
                        Console.WriteLine($"Server received: {completeMessage}");

                        // Отправка ответа клиенту
                        string responseMessage = "Message received!";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                        stream.Write(responseBytes, 0, responseBytes.Length);

                        Console.WriteLine("Response sent.");

                        // Очищаем буфер для следующего сообщения
                        messageBuffer.Clear();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while handling client: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Connection closed.");
        }
    }
}
