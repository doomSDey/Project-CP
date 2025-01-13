import { Sha256 } from '@aws-crypto/sha256-js';
import { HttpRequest } from '@aws-sdk/protocol-http';
import { SignatureV4 } from '@aws-sdk/signature-v4';
import type { NextApiRequest, NextApiResponse } from 'next';

const apiUrl = 'https://aco7wkvnqd.execute-api.ap-south-1.amazonaws.com/Prod/Project-CP';
const accessKey = process.env.AWS_ACCESS_KEY!;
const secretKey = process.env.AWS_SECRET_KEY!;
const region = 'ap-south-1';
const service = 'execute-api';

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
    throw new Error(`Error fetching scores: ${response.statusText}`);
  }

  return response.json();
}

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  try {
    const { limit = 10, page = 1, playerID } = req.query;
    const data = await fetchHighScores(Number(limit), Number(page), playerID as string);
    res.status(200).json(data);
  } catch (error: unknown) {
    if (error instanceof Error) {
      res.status(500).json({ error: error.message });
    } else {
      res.status(500).json({ error: 'An unknown error occurred' });
    }
  }
}
