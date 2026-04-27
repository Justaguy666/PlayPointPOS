export function errorHandler(error, _request, response, _next) {
  const statusCode = Number.isInteger(error.statusCode) ? error.statusCode : 500;
  const code = error.code ?? (statusCode >= 500 ? "INTERNAL_SERVER_ERROR" : "REQUEST_ERROR");

  response.status(statusCode).json({
    error: {
      code,
      message: statusCode >= 500 ? "An unexpected error occurred." : error.message
    }
  });
}
