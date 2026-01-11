export function newlineToBr(text: string): string {
  return text.replace(/\n/g, '<br/>')
}

// Utility function to create a readable title from a section ID.
export function niceify(id: string): string {
  return id
    .split('_')
    .map((word: string) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
    .join(' ')
}
