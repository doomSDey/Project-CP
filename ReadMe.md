
# Capywarz (Codename: Project-CP)

![Title Image](https://d112y698adiu2z.cloudfront.net/photos/production/software_photos/003/211/125/datas/original.png)

A web-based arcade shooter game with a global leaderboard system powered by AWS serverless architecture.

## 🎮 Play the Game

[Play Project-CP](https://d1670hkpds66xb.cloudfront.net/)

[Read about the journey](https://community.aws/editor/posts/preview/content/2rbgYlmzSHXfq8b4e86i62apzKf?v=2rbgYs3OBN1JSbiznCsrc7EoEmP)

![Gameplay](https://d112y698adiu2z.cloudfront.net/photos/production/software_photos/003/214/263/datas/original.PNG)

![Gameplay](https://d112y698adiu2z.cloudfront.net/photos/production/software_photos/003/214/401/datas/original.PNG)
## 📋 Overview

Project-CP is a web game that features:
- Online gameplay
- Global leaderboard system
- Real-time score tracking
- Paginated high scores
- Player-specific score filtering

## 🏗️ Architecture

The project uses a serverless architecture with the following components:

- **Frontend**: Hosted on Amazon App Runner, cached using Amazon CloudFront
- **Content Storage**: Amazon S3 for Unity build and game assets
- **Backend**: AWS Lambda for backend logic
- **Database**: Amazon DynamoDB for high scores
- **API**: REST API managed by Amazon API Gateway with CORS support

## 🔧 Technical Stack

### Directories
- **Unity**: Contains the game build files for WebGL
- **Lambda**: Contains the backend logic for handling high scores
- **Webapp**: Contains the Next.js frontend

### Frontend
- **Framework**: Next.js
- **Hosting**: Amazon App Runner

### Backend
- **Language**: Python 3.x
- **Services**: AWS Lambda, Amazon DynamoDB
- **Libraries**: AWS SDK (boto3)

## 🌟 Features

### High Score System
- View global leaderboard
- Filter scores by player ID
- Paginated results
- Real-time rank calculation

### API Endpoints

1. **Insert High Score**
   - **Action**: `insert`
   - **Parameters**:
     - `PlayerID` (required)
     - `HighScore` (required)
   - **Response**: Player's score and global rank

2. **Get Scores**
   - **Action**: `get_scores`
   - **Parameters**:
     - `Limit` (optional, default: 10)
     - `PageNumber` (optional, default: 1)
     - `PlayerID` (optional, for filtering)
   - **Response**: Paginated list of scores

## 🔒 Security

- CORS enabled with specific origins
- Error handling and input validation
- Secure integration with AWS services using IAM roles and policies

## 🚀 Deployment and Testing

### Frontend
1. Clone the repository and navigate to the `Webapp` directory.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run the development server:
   ```bash
   npm run dev
   ```
4. For deployment:
   ```bash
   docker build -t project-cp-frontend .
   docker push <your-ecr-repo>
   ```

### Backend
1. Navigate to the `Lambda` directory.
2. Package the Lambda function:
   ```bash
   zip -r function.zip .
   ```
3. Deploy the function to AWS Lambda.

### Database
- Create a DynamoDB table with the schema:
  ```json
  {
      "PlayerID": "string",
      "Timestamp": "string (ISO format)",
      "HighScore": "number"
  }
  ```
- Define the table name in the Lambda function.

### Unity Build
1. Open the Unity project in the `Unity` directory.
2. Build the game for WebGL.
3. Upload the build files to the designated S3 bucket.

## 📝 Environment Variables

### Lambda Function
The `table_name` must be defined directly in the Lambda function code.

### Webapp
The following environment variables must be defined in a `.env` file, which is provided as `.env.example` in the `Webapp` directory:
- `AWS_ACCESS_KEY`
- `AWS_SECRET_KEY`

## Contributing

Contributions are welcome! Feel free to submit issues and enhancement requests. If you have suggestions for improving the architecture or code, open a pull request.
