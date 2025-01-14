export const dynamic = 'force-dynamic'; // Ensures dynamic responses and bypasses Next.js caching.

import { Sha256 } from '@aws-crypto/sha256-js';
import { HttpRequest } from '@aws-sdk/protocol-http';
import { SignatureV4 } from '@aws-sdk/signature-v4';
import type { NextApiRequest, NextApiResponse } from 'next';

// AWS credentials and configuration
const accessKey = process.env.AWS_ACCESS_KEY!;
const secretKey = process.env.AWS_SECRET_KEY!;
const region = 'ap-south-1';
const service = 'execute-api';
const apiUrl = 'https://aco7wkvnqd.execute-api.ap-south-1.amazonaws.com/Prod/Project-CP';

// Function to fetch high scores with AWS Signature V4 authentication
async function fetchHighScores(limit: number, page: number, playerID?: string) {
  const payload = JSON.stringify({
    action: 'get_scores',
    Limit: limit,
    PageNumber: page,
    ...(playerID && { PlayerID: playerID }),
  });

  const httpRequest = new HttpRequest({
    method: 'POST',
    hostname: 'aco7wkvnqd.execute-api.ap-south-1.amazonaws.com',
    path: '/Prod/Project-CP',
    headers: {
      'Content-Type': 'application/json',
      host: 'aco7wkvnqd.execute-api.ap-south-1.amazonaws.com',
    },
    body: payload,
  });

  const signer = new SignatureV4({
    credentials: {
      accessKeyId: accessKey,
      secretAccessKey: secretKey,
    },
    region,
    service,
    sha256: Sha256,
  });

  const signedRequest = await signer.sign(httpRequest);

  const response = await fetch(apiUrl, {
    method: 'POST',
    headers: signedRequest.headers,
    body: payload,
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch scores: ${response.statusText}`);
  }

  return response.json();
}

// Main API handler
export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  try {
    const { limit = 10, page = 1, playerID } = req.query;

    // Fetch high scores
    const data = await fetchHighScores(Number(limit), Number(page), playerID as string);

    // Set Cache-Control headers to prevent CloudFront from caching
    res.setHeader('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
    res.setHeader('Pragma', 'no-cache');
    res.setHeader('Expires', '0');

    // Return the data
    res.status(200).json(data);
  } catch (error: unknown) {
    console.error('Error fetching high scores:', error);
    res.status(500).json({ error: 'An error occurred while fetching high scores.' });
  }
}
