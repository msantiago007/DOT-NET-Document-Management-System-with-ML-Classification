# DocumentManagementML API Instructions

## Starting the API

### From Windows PowerShell
To start the API from Windows PowerShell, run the following command:
```powershell
wsl -e bash -c "cd /home/administrator/nodejs/DocumentManagementML && ./start-api.sh"
```

### From WSL Terminal
If you're directly in the WSL terminal, navigate to the project directory and run:
```bash
cd /home/administrator/nodejs/DocumentManagementML
./start-api.sh
```

This will start the API server on port 5149, binding to all available network interfaces.

## Accessing the API

### Swagger UI
The easiest way to explore and test the API is through Swagger UI, which is available at:
```
http://localhost:5149/swagger
```

### Available Endpoints

#### Document Types
- `GET /api/DocumentTypes` - Get all document types
- `GET /api/DocumentTypes/{id}` - Get a specific document type
- `POST /api/DocumentTypes` - Create a new document type
- `PUT /api/DocumentTypes/{id}` - Update a document type
- `DELETE /api/DocumentTypes/{id}` - Delete a document type

#### Documents
- `GET /api/Documents` - Get all documents
- `GET /api/Documents/{id}` - Get a specific document
- `POST /api/Documents` - Upload a new document
- `PUT /api/Documents/{id}` - Update a document
- `DELETE /api/Documents/{id}` - Delete a document
- `POST /api/Documents/{id}/classify` - Classify a document

#### ML Endpoints
- `GET /api/ML/metrics` - Get model metrics
- `POST /api/ML/train` - Train the model
- `POST /api/ML/classify` - Classify a document

## Notes on Networking
If you're running the API in WSL and can't access it from your Windows browser:
1. Find the WSL IP address by running `hostname -I` in your WSL terminal
2. Access the API using that IP instead of localhost: `http://<WSL-IP>:5149/swagger`

## Troubleshooting
- If you can't connect to the API, ensure no firewall is blocking port 5149
- For WSL users, make sure you're using the correct IP address to access the service
- Check the console output for any error messages when starting the API