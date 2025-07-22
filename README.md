# 🔧 OficinaMVC: Your Workshop Management Solution 🛠️

OficinaMVC is a comprehensive ASP.NET Core web application designed to streamline the management of a vehicle workshop or repair shop. It provides features for managing services, repairs, appointments, user roles, and more. It aims to improve efficiency and organization for automotive service businesses.

## 🚀 Key Features

- **User Authentication and Authorization**: Secure user management with role-based access control using ASP.NET Core Identity.
- **Service Management**: Categorize and manage different types of services offered (Maintenance, Repair, Inspection, etc.).
- **Repair Tracking**: Track the status of repair jobs (Open, Waiting for Parts, In Progress, Completed, Cancelled).
- **Appointment Scheduling**: Manage appointments with different statuses (Pending, Confirmed, In Progress, Completed, Cancelled).
- **Vehicle Management**: Store and manage vehicle information, including fuel type.
- **Email Integration**: Send email notifications and confirmations.
- **Background Job Processing**: Handle long-running tasks using Hangfire.
- **Real-time Communication**: Utilize SignalR for real-time updates and notifications.
- **HTML to PDF Conversion**: Generate PDF reports using DinkToPdf.
- **Configuration**: Flexible configuration using `appsettings.json` for different environments.

## 🛠️ Tech Stack

| Category      | Technology                                  | Description                                                                 |
|---------------|---------------------------------------------|-----------------------------------------------------------------------------|
| **Frontend**  | Razor Pages, HTML, CSS, JavaScript          | User interface for interacting with the application.                         |
| **Backend**   | ASP.NET Core 8.0                            | Web framework for building the application.                                 |
| **Database**  | Microsoft SQL Server                        | Relational database for storing application data.                           |
| **ORM**       | Entity Framework Core                       | Object-relational mapper for interacting with the database.                 |
| **Identity**  | ASP.NET Core Identity                       | Framework for user authentication and authorization.                        |
| **Realtime**  | SignalR                                     | Library to add real-time web functionalities.                               |
| **Background Jobs** | Hangfire                                  | Easy way to perform background processing in .NET applications.              |
| **PDF Generation** | DinkToPdf                                 | Library for converting HTML to PDF.                                         |
| **Email**     | MailKit                                     | Cross-platform .NET library for POP3, IMAP, and SMTP.                       |
| **Build Tool**| .NET SDK                                    | Used for building, running, and deploying .NET applications.                |
| **Configuration** | JSON (`appsettings.json`)                 | Used for storing configuration settings.                                    |
| **Interoperability** | IKVM                                    | .NET implementation of Java, used for Java interoperability.                |
| **Authentication** | JWT (JSON Web Tokens)                     | Standard for securely transmitting information between parties as a JSON object |

## 📦 Getting Started

Follow these instructions to get OficinaMVC up and running on your local machine.

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download)

### Installation

1.  **Clone the repository:**

    ```bash
    git clone <repository_url>
    cd OficinaMVC
    ```

2.  **Update Database Connection String:**

    Modify the `ConnectionStrings` section in `appsettings.json` and `appsettings.Development.json` with your SQL Server connection details.

    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=your_server;Database=OficinaMVCDb;User Id=your_user_id;Password=your_password;TrustServerCertificate=True;"
    }
    ```

3.  **Apply Database Migrations:**

    Open a terminal in the project directory and run the following commands:

    ```bash
    dotnet tool install --global dotnet-ef
    dotnet ef database update
    ```

4.  **Install wkhtmltopdf:**

    Install `wkhtmltopdf` to your system and ensure that `libwkhtmltox.dll` is present in your project directory. This is required for the DinkToPdf library to function correctly.

### Running Locally

1.  **Build the application:**

    ```bash
    dotnet build
    ```

2.  **Run the application:**

    ```bash
    dotnet run
    ```

3.  **Access the application:**

    Open your web browser and navigate to `https://localhost:<port>`, where `<port>` is the port number specified in the application's configuration (typically 5001 for HTTPS).

## 📂 Project Structure

```
OficinaMVC/
├── OficinaMVC.csproj           # Project file
├── appsettings.json            # Application configuration
├── appsettings.Development.json # Development environment configuration
├── Program.cs                  # Entry point of the application
├── Models/
│   └── Enums/
│       ├── ServiceType.cs      # Enum for service types
│       ├── FuelType.cs         # Enum for fuel types
│       ├── RepairStatus.cs     # Enum for repair statuses
│       └── AppointmentStatus.cs# Enum for appointment statuses
├── Helpers/
│   ├── ConverterHelper.cs    # Helper for converting between DTOs and entities
│   ├── IConverterHelper.cs   # Interface for ConverterHelper
│   ├── UserHelper.cs         # Helper for user management
│   └── IUserHelper.cs        # Interface for UserHelper
├── ...                       # Other directories and files
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and commit them with descriptive messages.
4.  Submit a pull request.

## 📝 License

This project is licensed under the [MIT License](LICENSE).

## 📬 Contact

For questions or feedback, please contact: Frederico Santos - fred.sousa.santos@gmail.com

## 💖 Thanks

Thank you for checking out OficinaMVC! We hope it helps you manage your workshop more efficiently.
