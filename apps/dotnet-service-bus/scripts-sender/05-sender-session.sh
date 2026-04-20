# !/bin/bash
# Sender script to test session-based message sending. It sends 30 requests, cycling through 3 different sessions (session1, session2, session3). 
# Each request sends a batch of 10 messages with a specific sessionId.
for i in {1..30}; do
  session=$((($i - 1) % 3 + 1))
  session_message_counter=$((($i - 1) / 3 + 1))
  echo "Request $i - Session $session - Message $session_message_counter"
  curl -s "http://localhost:5141/send?message=session${session}--message-${session_message_counter}&num=1&sessionId=session${session}"
  echo ""
done