'use client';

import React, { useEffect,useState } from 'react';

import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';

const HighScoresTable: React.FC = () => {
  interface Score {
    PlayerID: string;
    HighScore: number;
  }

  const [scores, setScores] = useState<Score[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [playerID, setPlayerID] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchScores = async (page: number, playerID = '') => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`/api/highscores?limit=10&page=${page}&playerID=${playerID}`);
      const data = await response.json();

      setScores(data.Scores || []);
      setTotalPages(data.TotalPages || 1);
      setCurrentPage(page);
    } catch (err) {
      setError('Failed to fetch scores');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchScores(1);
  }, []);

  const handleSearch = () => {
    fetchScores(1, playerID);
  };

  const handlePageChange = (page: number) => {
    fetchScores(page, playerID);
  };

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">High Scores</h1>

      <div className="mb-4 flex gap-2">
        <input
          type="text"
          placeholder="Search by PlayerID"
          value={playerID}
          onChange={(e) => setPlayerID(e.target.value)}
          className="border border-gray-300 rounded px-2 py-1"
        />
        <button
          onClick={handleSearch}
          className="bg-blue-500 text-white px-4 py-2 rounded"
        >
          Search
        </button>
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : error ? (
        <p className="text-red-500">{error}</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Rank</TableHead>
              <TableHead>PlayerID</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {scores.length > 0 ? (
              scores.map((score: Score, index: number) => (
                <TableRow key={`${score.PlayerID}-${index}`}>
                  <TableCell>{(currentPage - 1) * 10 + index + 1}</TableCell>
                  <TableCell>{score.PlayerID}</TableCell>
                  <TableCell>{score.HighScore}</TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={3} className="text-center">
                  No scores found
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}

      <div className="mt-4 flex justify-between">
        <button
          disabled={currentPage === 1}
          onClick={() => handlePageChange(currentPage - 1)}
          className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
        >
          Previous
        </button>
        <span>
          Page {currentPage} of {totalPages}
        </span>
        <button
          disabled={currentPage === totalPages}
          onClick={() => handlePageChange(currentPage + 1)}
          className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default HighScoresTable;
