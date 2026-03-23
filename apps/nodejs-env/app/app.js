// Simple web server to return environment details
const http = require('http');
const url = require('url');
const envinfo = require('envinfo');

const PORT = process.env.PORT || 3000;
const HOST = process.env.HOST || '0.0.0.0';

// Helper function to send JSON response
function sendJSON(res, statusCode, data) {
  res.writeHead(statusCode, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(data, null, 2));
}

// Create HTTP server
const server = http.createServer(async (req, res) => {
  const parsedUrl = url.parse(req.url, true);
  const pathname = parsedUrl.pathname;
  const query = parsedUrl.query;

  console.log(`${new Date().toISOString()} - ${req.method} ${req.url}`);

  // Route: GET /
  if (pathname === '/' && req.method === 'GET') {
    sendJSON(res, 200, {
      message: 'Environment Information API',
      endpoints: {
        '/': 'This help message',
        '/version': 'API version information',
        '/health': 'Health check endpoint',
        '/env': 'Get all environment variables',
        '/env/:key': 'Get specific environment variable (e.g., /env/PATH)',
        '/info': 'Get detailed system and environment information',
        '/process': 'Get Node.js process information'
      }
    });
  }
  
  // Route: GET /version
  else if (pathname === '/version' && req.method === 'GET') {
    sendJSON(res, 200, {
      version: 'v1.0.0',

    });
  }

  // Route: GET /health
  else if (pathname === '/health' && req.method === 'GET') {
    sendJSON(res, 200, {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      uptime: process.uptime()
    });
  }
  
  // Route: GET /env
  else if (pathname === '/env' && req.method === 'GET') {
    sendJSON(res, 200, {
      environment: process.env
    });
  }
  
  // Route: GET /env/:key
  else if (pathname.startsWith('/env/') && req.method === 'GET') {
    const key = pathname.substring(5); // Remove '/env/' prefix
    if (process.env.hasOwnProperty(key)) {
      sendJSON(res, 200, {
        key: key,
        value: process.env[key]
      });
    } else {
      sendJSON(res, 404, {
        error: 'Environment variable not found',
        key: key
      });
    }
  }
  
  // Route: GET /info
  else if (pathname === '/info' && req.method === 'GET') {
    try {
      const info = await envinfo.run(
        {
          System: ['OS', 'CPU', 'Memory', 'Shell'],
          Binaries: ['Node', 'npm', 'Yarn'],
          Browsers: ['Chrome', 'Firefox', 'Safari'],
          npmPackages: ['*'],
          npmGlobalPackages: ['npm', 'typescript']
        },
        { showNotFound: true, json: true }
      );
      
      sendJSON(res, 200, {
        systemInfo: JSON.parse(info),
        nodeVersion: process.version,
        platform: process.platform,
        arch: process.arch
      });
    } catch (error) {
      sendJSON(res, 500, {
        error: 'Failed to get system information',
        message: error.message
      });
    }
  }
  
  // Route: GET /process
  else if (pathname === '/process' && req.method === 'GET') {
    sendJSON(res, 200, {
      pid: process.pid,
      version: process.version,
      platform: process.platform,
      arch: process.arch,
      uptime: process.uptime(),
      argv: process.argv,
      execPath: process.execPath,
      cwd: process.cwd(),
      memoryUsage: process.memoryUsage(),
      cpuUsage: process.cpuUsage()
    });
  }
  
  // Route: 404 Not Found
  else {
    sendJSON(res, 404, {
      error: 'Not Found',
      path: pathname
    });
  }
});

// Start server
server.listen(PORT, HOST, () => {
  console.log(`Server running at http://${HOST}:${PORT}/`);
  console.log(`\nAvailable endpoints:`);
  console.log(`  GET /           - API help`);
  console.log(`  GET /health     - Health check`);
  console.log(`  GET /env        - All environment variables`);
  console.log(`  GET /env/:key   - Specific environment variable`);
  console.log(`  GET /info       - System information`);
  console.log(`  GET /process    - Node.js process information`);
  console.log(`\nPress Ctrl+C to stop the server\n`);
});

// Handle graceful shutdown
process.on('SIGTERM', () => {
  console.log('SIGTERM received, shutting down gracefully...');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});

process.on('SIGINT', () => {
  console.log('\nSIGINT received, shutting down gracefully...');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});