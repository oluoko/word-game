// Simple Node.js Express Backend for Word Guess Multiplayer
// Install dependencies: npm install express body-parser cors

const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());

// In-memory storage (replace with database in production)
let challenges = [];
let results = [];
let players = new Map();

// Helper function to generate ID
function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2);
}

// Routes

// Get all challenges for a player
app.get("/api/challenges", (req, res) => {
  const { playerId, playerName } = req.query;

  if (!playerId && !playerName) {
    return res.status(400).json({ error: "Player ID or name required" });
  }

  // Filter challenges for this player
  const playerChallenges = challenges.filter(
    (c) => c.toPlayer === playerName || c.toPlayerId === playerId,
  );

  res.json({
    success: true,
    challenges: playerChallenges,
  });
});

// Create a new challenge
app.post("/api/challenges", (req, res) => {
  const { fromPlayer, toPlayer, targetWord, difficulty, clue } = req.body;

  if (!fromPlayer || !toPlayer || !targetWord) {
    return res.status(400).json({
      error: "Missing required fields: fromPlayer, toPlayer, targetWord",
    });
  }

  const challenge = {
    challengeId: generateId(),
    fromPlayer,
    toPlayer,
    targetWord: targetWord.toUpperCase(),
    difficulty: difficulty || 1,
    clue: clue || "",
    completed: false,
    attempts: 0,
    timestamp: Date.now(),
    createdAt: new Date().toISOString(),
  };

  challenges.push(challenge);

  res.json({
    success: true,
    challenge,
  });
});

// Get a specific challenge
app.get("/api/challenges/:challengeId", (req, res) => {
  const { challengeId } = req.params;

  const challenge = challenges.find((c) => c.challengeId === challengeId);

  if (!challenge) {
    return res.status(404).json({ error: "Challenge not found" });
  }

  res.json({
    success: true,
    challenge,
  });
});

// Submit challenge result
app.post("/api/results", (req, res) => {
  const { challengeId, playerId, won, attempts } = req.body;

  if (!challengeId || !playerId || won === undefined) {
    return res.status(400).json({
      error: "Missing required fields: challengeId, playerId, won",
    });
  }

  const result = {
    resultId: generateId(),
    challengeId,
    playerId,
    won,
    attempts,
    completionTime: Date.now(),
    completedAt: new Date().toISOString(),
  };

  results.push(result);

  // Mark challenge as completed
  const challenge = challenges.find((c) => c.challengeId === challengeId);
  if (challenge) {
    challenge.completed = true;
    challenge.attempts = attempts;
  }

  res.json({
    success: true,
    result,
  });
});

// Get leaderboard
app.get("/api/leaderboard", (req, res) => {
  const { limit = 10 } = req.query;

  // Calculate player stats
  const playerStats = new Map();

  results.forEach((result) => {
    if (!playerStats.has(result.playerId)) {
      playerStats.set(result.playerId, {
        playerId: result.playerId,
        gamesPlayed: 0,
        gamesWon: 0,
        totalAttempts: 0,
        bestStreak: 0,
        currentStreak: 0,
      });
    }

    const stats = playerStats.get(result.playerId);
    stats.gamesPlayed++;
    if (result.won) {
      stats.gamesWon++;
      stats.currentStreak++;
      stats.bestStreak = Math.max(stats.bestStreak, stats.currentStreak);
    } else {
      stats.currentStreak = 0;
    }
    stats.totalAttempts += result.attempts;
  });

  // Convert to array and sort by win rate
  const leaderboard = Array.from(playerStats.values())
    .map((stats) => ({
      ...stats,
      winRate:
        stats.gamesPlayed > 0
          ? ((stats.gamesWon / stats.gamesPlayed) * 100).toFixed(1)
          : 0,
      avgAttempts:
        stats.gamesPlayed > 0
          ? (stats.totalAttempts / stats.gamesPlayed).toFixed(1)
          : 0,
    }))
    .sort((a, b) => {
      // Sort by win rate, then by best streak
      if (b.winRate !== a.winRate) {
        return b.winRate - a.winRate;
      }
      return b.bestStreak - a.bestStreak;
    })
    .slice(0, parseInt(limit));

  res.json({
    success: true,
    leaderboard,
  });
});

// Get player statistics
app.get("/api/players/:playerId/stats", (req, res) => {
  const { playerId } = req.params;

  const playerResults = results.filter((r) => r.playerId === playerId);

  const stats = {
    playerId,
    gamesPlayed: playerResults.length,
    gamesWon: playerResults.filter((r) => r.won).length,
    totalAttempts: playerResults.reduce((sum, r) => sum + r.attempts, 0),
    avgAttempts: 0,
    winRate: 0,
    bestStreak: 0,
    challengesSent: challenges.filter((c) => c.fromPlayer === playerId).length,
    challengesReceived: challenges.filter((c) => c.toPlayer === playerId)
      .length,
  };

  if (stats.gamesPlayed > 0) {
    stats.avgAttempts = (stats.totalAttempts / stats.gamesPlayed).toFixed(1);
    stats.winRate = ((stats.gamesWon / stats.gamesPlayed) * 100).toFixed(1);
  }

  // Calculate best streak
  let currentStreak = 0;
  let bestStreak = 0;
  playerResults
    .sort((a, b) => a.completionTime - b.completionTime)
    .forEach((result) => {
      if (result.won) {
        currentStreak++;
        bestStreak = Math.max(bestStreak, currentStreak);
      } else {
        currentStreak = 0;
      }
    });

  stats.bestStreak = bestStreak;

  res.json({
    success: true,
    stats,
  });
});

// Delete a challenge (for testing)
app.delete("/api/challenges/:challengeId", (req, res) => {
  const { challengeId } = req.params;

  const index = challenges.findIndex((c) => c.challengeId === challengeId);

  if (index === -1) {
    return res.status(404).json({ error: "Challenge not found" });
  }

  challenges.splice(index, 1);

  res.json({
    success: true,
    message: "Challenge deleted",
  });
});

// Health check
app.get("/api/health", (req, res) => {
  res.json({
    success: true,
    status: "Server is running",
    timestamp: new Date().toISOString(),
    stats: {
      totalChallenges: challenges.length,
      totalResults: results.length,
      activeChallenges: challenges.filter((c) => !c.completed).length,
    },
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`Word Guess API server running on port ${PORT}`);
  console.log(`Health check: http://localhost:${PORT}/api/health`);
});

// Export for testing
module.exports = app;
