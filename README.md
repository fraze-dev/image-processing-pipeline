# 🖼️ Azure Image Processing Pipeline

A serverless, event-driven image processing system built with Azure Functions, demonstrating modern cloud architecture patterns and Azure services integration.

![Azure](https://img.shields.io/badge/Azure-Functions-0078D4?logo=microsoft-azure)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-Language-239120?logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-green)

## 📋 Overview

This project implements an automated image processing pipeline that:
- Detects uploaded images in Azure Blob Storage
- Automatically resizes and optimizes images
- Generates thumbnails
- Tracks metadata in Azure Table Storage
- Sends notifications via Azure Queue Storage
- Processes notifications asynchronously

**Portfolio Project** | Built for AZ-204 certification preparation

---

## 🏗️ Architecture
```
User Upload → Blob Storage (uploads)
                    ↓
        [ProcessImageFunction - Blob Trigger]
                    ↓
            ┌───────┴───────┐
            ↓               ↓
    Image Processing    Queue Message
    (Resize + Thumb)    (Notification)
            ↓               ↓
    ┌───────┴────┐         ↓
    ↓            ↓         ↓
Blob Storage  Table     [ProcessNotificationFunction - Queue Trigger]
(processed,  Storage         ↓
thumbnails) (metadata)   Logging/Alerts
```

---

## 🛠️ Technologies Used

### Azure Services
- **Azure Functions** (.NET 8 Isolated) - Serverless compute
- **Azure Blob Storage** - Image storage (uploads, processed, thumbnails)
- **Azure Table Storage** - Metadata tracking
- **Azure Queue Storage** - Asynchronous messaging
- **Azure Function App** - Hosting environment

### Development Stack
- **.NET 8** - Runtime framework
- **C#** - Primary language
- **ImageSharp** - Image processing library
- **Azure SDK** - Storage client libraries

---

## ✨ Features

- ✅ **Event-Driven Architecture** - Automatic processing on upload
- ✅ **Image Optimization** - Resizes images to max 800px width while maintaining aspect ratio
- ✅ **Thumbnail Generation** - Creates 200x200 square thumbnails
- ✅ **Metadata Tracking** - Stores processing details in Table Storage
- ✅ **Compression Statistics** - Tracks original vs. processed file sizes
- ✅ **Queue-Based Notifications** - Asynchronous notification processing
- ✅ **Serverless & Scalable** - Auto-scales based on load
- ✅ **Cost-Efficient** - Pay-per-execution pricing model

---

## 📊 Sample Results

**Example Processing:**
- Original: 410,632 bytes (410 KB)
- Processed: 66,933 bytes (66 KB)
- **Compression: 83.7%** 🎯

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- Azure Subscription (free tier works!)

### Local Setup

1. **Clone the repository**
```bash
   git clone https://github.com/YOUR_USERNAME/ImageProcessingPipeline.git
   cd ImageProcessingPipeline
```

2. **Install dependencies**
```bash
   dotnet restore
```

3. **Create Azure Storage Account**
```bash
   az login
   az group create --name rg-imageprocessing --location eastus2
   az storage account create \
     --name stgimgproc$(Get-Random -Maximum 9999) \
     --resource-group rg-imageprocessing \
     --location eastus2 \
     --sku Standard_LRS
```

4. **Create storage containers and queue**
```bash
   # Get connection string
   $connectionString = az storage account show-connection-string \
     --name YOUR_STORAGE_ACCOUNT_NAME \
     --resource-group rg-imageprocessing \
     --query connectionString \
     --output tsv

   # Create containers
   az storage container create --name uploads --connection-string $connectionString
   az storage container create --name processed --connection-string $connectionString
   az storage container create --name thumbnails --connection-string $connectionString
   
   # Create queue
   az storage queue create --name image-processing-queue --connection-string $connectionString
```

5. **Configure local settings**
```bash
   cp local.settings.template.json local.settings.json
   # Edit local.settings.json and add your connection string
```

6. **Run locally**
```bash
   func start
```

7. **Test the pipeline**
   - Upload an image to the `uploads` container
   - Watch the console for processing logs
   - Check `processed` and `thumbnails` containers for results

---

## ☁️ Deploy to Azure

1. **Create Function App**
```bash
   az functionapp create \
     --resource-group rg-imageprocessing \
     --consumption-plan-location eastus2 \
     --runtime dotnet-isolated \
     --runtime-version 8 \
     --functions-version 4 \
     --name func-imageproc-$(Get-Random -Maximum 9999) \
     --storage-account YOUR_STORAGE_ACCOUNT_NAME
```

2. **Configure app settings**
```bash
   az functionapp config appsettings set \
     --name YOUR_FUNCTION_APP_NAME \
     --resource-group rg-imageprocessing \
     --settings "StorageConnectionString=$connectionString"
```

3. **Deploy**
```bash
   func azure functionapp publish YOUR_FUNCTION_APP_NAME
```

---

## 📁 Project Structure
```
ImageProcessingPipeline/
├── Models/
│   └── ImageMetadata.cs          # Table Storage entity
├── Services/
│   └── ImageProcessor.cs         # Image processing logic
├── ProcessImageFunction.cs       # Blob trigger function
├── ProcessNotificationFunction.cs # Queue trigger function
├── Program.cs                    # Host configuration
├── host.json                     # Function host settings
├── local.settings.json          # Local configuration (gitignored)
└── ImageProcessingPipeline.csproj
```

---

## 🎓 AZ-204 Exam Coverage

This project demonstrates key AZ-204 exam objectives:

### ✅ Develop Azure compute solutions (25-30%)
- Implement Azure Functions
- Blob-triggered functions
- Queue-triggered functions

### ✅ Develop for Azure storage (15-20%)
- Azure Blob Storage operations
- Azure Table Storage
- Azure Queue Storage
- Storage SDKs

### ✅ Implement Azure security (20-25%)
- Connection string management
- Secure configuration

### ✅ Monitor, troubleshoot, and optimize (15-20%)
- Logging and diagnostics
- Performance optimization (image compression)

---

## 📝 What I Learned

- Event-driven architecture patterns in Azure
- Azure Functions isolated worker model (.NET 8)
- Multi-storage service integration (Blob, Table, Queue)
- Image processing with ImageSharp
- Asynchronous message processing
- Serverless deployment and configuration
- Azure CLI automation

---

## 🔜 Future Enhancements

- [ ] Add Application Insights for advanced monitoring
- [ ] Implement retry policies and dead-letter queue handling
- [ ] Add support for multiple image formats
- [ ] Implement batch processing
- [ ] Add user authentication and authorization
- [ ] Create a web UI for uploads and viewing

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 👤 Author

**Aaron**

---

## 🙏 Acknowledgments

Built as part of AZ-204 certification preparation. Special thanks to the Azure and .NET communities.

---

**⭐ If you found this project helpful, please give it a star!**