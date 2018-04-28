#include "GameEngine.h"
#include "gl/glew.h"
#include "gl/glut.h"
#include <stdio.h>
#include <stdlib.h>
#pragma comment(lib,"glew32.lib")

GLfloat mVertex[] = 
{
	0.1,0.1,1,1,0,1,
	0.2,0.2,1,1,0,1,
	0.1,0.2,1,1,0,1,
};

void Loop()
{
	glClearColor(0, 0, 0, 1);
	glClear(GL_COLOR_BUFFER_BIT);
	
	GLuint VBO;
	GLint OFFSET = 2 * sizeof(GL_FLOAT);
	glGenBuffers(1, &VBO);
	glBindBuffer(GL_ARRAY_BUFFER, VBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(GLfloat) * 6 * 3, mVertex, GL_STATIC_DRAW);
	glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE ,sizeof(GL_FLOAT) * 6, (const void*)0);
	glVertexAttribPointer(1, 4, GL_FLOAT, GL_FALSE, sizeof(GL_FLOAT) * 6, (const void*)(sizeof(GL_FLOAT) * 2));
	glEnableVertexAttribArray(0);
	glEnableVertexAttribArray(1);
	
	glDrawArrays(GL_TRIANGLES, 0, 3);
	glDeleteBuffers(1, &VBO);

	glutSwapBuffers();

	glReadBuffer(GL_FRONT);
	void* ptr = malloc(960 * 540 * 4 * 8);
	glReadPixels(0, 0, 960, 540, GL_UNSIGNED_BYTE, GL_RGBA, ptr);
	free(ptr);
}

void Idle()
{
	
}

int main(int argc,char** argv)
{
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH | GLUT_STENCIL);
	glutInitWindowPosition(0, 0);
	glutInitWindowSize(960, 540);
	glutCreateWindow("HELLO WORLD");
	glutDisplayFunc(Loop);
	glutIdleFunc(Idle);
	GLenum ret = glewInit();
	if (ret != GLEW_OK)
	{
		printf("glew init error %s", glewGetErrorString(ret));
	}
	else
	{
		glutMainLoop();
	}
	system("pause");
	return 0;
}