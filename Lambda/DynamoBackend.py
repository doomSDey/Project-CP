import math
import json
import boto3
from boto3.dynamodb.conditions import Key
from datetime import datetime
from decimal import Decimal

# Initialize DynamoDB client
dynamodb = boto3.resource('dynamodb')
table_name = 'Project-CP-Highscores'
table = dynamodb.Table(table_name)

def lambda_handler(event, context):
    print("Received event:", json.dumps(event, indent=2))  # Log for debugging

    # Parse the body if passed through API Gateway HTTP API
    body = event.get('body', '{}')
    if isinstance(body, str):
        body = json.loads(body)

    action = body.get('action', '')

    if action == 'insert':
        return insert_high_score(body)
    elif action == 'get_scores':
        return get_scores(body)
    else:
        return {
            'statusCode': 400,
            'body': json.dumps('Invalid action. Use "insert" or "get_scores".')
        }

def insert_high_score(event):
    player_id = event.get('PlayerID')
    high_score = event.get('HighScore')

    if not player_id or high_score is None:
        return {
            'statusCode': 400,
            'body': json.dumps('PlayerID and HighScore are required.')
        }

    timestamp = datetime.utcnow().isoformat()

    # Insert the new high score
    table.put_item(
        Item={
            'PlayerID': player_id,
            'Timestamp': timestamp,
            'HighScore': Decimal(str(high_score))
        }
    )

    # Fetch all scores to calculate the rank
    response = table.scan()
    all_scores = response['Items']

    # Sort scores by HighScore in descending order
    all_scores.sort(key=lambda x: x['HighScore'], reverse=True)

    # Assign ranks
    for rank, score in enumerate(all_scores, start=1):
        if score['PlayerID'] == player_id and score['HighScore'] == Decimal(str(high_score)) and score['Timestamp'] == timestamp:
            return {
                'statusCode': 200,
                'body': json.dumps({
                    'message': 'High score added successfully.',
                    'PlayerID': player_id,
                    'HighScore': high_score,
                    'Rank': rank
                })
            }

    return {
        'statusCode': 500,
        'body': json.dumps('Failed to calculate rank.')
    }

def get_scores(event):
    limit = event.get('Limit', 10)
    page_number = event.get('PageNumber', 1)
    player_id_filter = event.get('PlayerID', None)

    # Scan the table to get all scores
    response = table.scan()
    all_scores = response['Items']

    # Continue scanning if there are more pages of results
    while 'LastEvaluatedKey' in response:
        response = table.scan(ExclusiveStartKey=response['LastEvaluatedKey'])
        all_scores.extend(response['Items'])

    # Filter scores by PlayerID if provided
    if player_id_filter:
        all_scores = [score for score in all_scores if score['PlayerID'] == player_id_filter]

    if not all_scores:
        return {
            'statusCode': 404,
            'body': json.dumps({
                'message': f'No scores found for PlayerID: {player_id_filter}',
                'Scores': [],
                'CurrentPage': 0,
                'TotalPages': 0
            })
        }

    # Sort scores by HighScore in descending order
    all_scores.sort(key=lambda x: x['HighScore'], reverse=True)
    total_pages = math.ceil(len(all_scores) / limit)

    if page_number < 1 or page_number > total_pages:
        return {
            'statusCode': 400,
            'body': json.dumps(f'Invalid PageNumber. Must be between 1 and {total_pages}.')
        }

    start_index = (page_number - 1) * limit
    end_index = start_index + limit
    paginated_scores = all_scores[start_index:end_index]

    return {
        'statusCode': 200,
        'body': json.dumps({
            'Scores': paginated_scores,
            'CurrentPage': page_number,
            'TotalPages': total_pages
        }, cls=DecimalEncoder)
    }

class DecimalEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Decimal):
            return float(obj)
        return super(DecimalEncoder, self).default(obj)