import { vi } from 'vitest'

export function stubApi(routes: Record<string, unknown>) {
  const fetchMock = vi.fn(async (path: string) => {
    if (!(path in routes)) return new Response(null, { status: 404 })

    const answer = routes[path]

    return typeof answer === 'number' ? new Response(null, { status: answer }) : Response.json(answer)
  })

  vi.stubGlobal('fetch', fetchMock)

  return fetchMock
}
