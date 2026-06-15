# Next Steps

[ ] Replication: Change the Coordinator to assign each block to multiple storage nodes (e.g., Primary and Secondary) to ensure data redundancy.
[ ] gRPC Streaming: swap out HTTP/REST web endpoints for gRPC
[x] Download: Implement the ability for the client to retrieve files from the server
[x] Aggregration: Merge blocks on download
[x] Heartbeats: Have the Coordinator ping storage nodes every few seconds to remove dead nodes from the pool automatically.
