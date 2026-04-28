import { describe, expect, it } from 'vitest'
import { AxiosError, AxiosHeaders } from 'axios'
import {
  getRetryDelay,
  normalizeApiError,
  shouldRetryRequest,
} from './apiError'

describe('normalizeApiError', () => {
  it.each([
    [400, 'The request is invalid. Please check your input.'],
    [401, 'Your session has expired. Please sign in again.'],
    [403, 'You do not have permission to perform this action.'],
    [404, 'The requested resource was not found.'],
    [409, 'This action conflicts with the current data. Please refresh and try again.'],
    [500, 'Something went wrong on our side. Please try again later.'],
  ])('returns a standard user message for HTTP %s', (status, message) => {
    const error = new AxiosError('Request failed', undefined, undefined, undefined, {
      status,
      statusText: String(status),
      headers: {},
      config: { headers: new AxiosHeaders() },
      data: {},
    })

    expect(normalizeApiError(error).message).toBe(message)
    expect(normalizeApiError(error).status).toBe(status)
  })

  it('uses API-provided message when available', () => {
    const error = new AxiosError('Request failed', undefined, undefined, undefined, {
      status: 400,
      statusText: 'Bad Request',
      headers: {},
      config: { headers: new AxiosHeaders() },
      data: { message: 'Title is required.', code: 'validation_error' },
    })

    expect(normalizeApiError(error)).toMatchObject({
      code: 'validation_error',
      message: 'Title is required.',
      status: 400,
      retryable: false,
    })
  })

  it('handles network errors as retryable', () => {
    const error = new AxiosError('Network Error')

    expect(normalizeApiError(error)).toMatchObject({
      code: 'network_error',
      message: 'Network error. Please check your connection and try again.',
      retryable: true,
    })
  })
})

describe('request retry behavior', () => {
  it('retries network errors and HTTP 5xx only for idempotent requests', () => {
    expect(shouldRetryRequest(new AxiosError('Network Error'), 0, 'get')).toBe(true)
    expect(shouldRetryRequest(new AxiosError('Network Error'), 2, 'get')).toBe(false)

    const serverError = new AxiosError('Request failed', undefined, undefined, undefined, {
      status: 500,
      statusText: 'Internal Server Error',
      headers: {},
      config: { headers: new AxiosHeaders() },
      data: {},
    })

    expect(shouldRetryRequest(serverError, 0, 'get')).toBe(true)
    expect(shouldRetryRequest(serverError, 0, 'post')).toBe(false)
  })

  it('uses bounded exponential retry delays', () => {
    expect(getRetryDelay(0)).toBe(500)
    expect(getRetryDelay(1)).toBe(1000)
    expect(getRetryDelay(5)).toBe(4000)
  })
})
